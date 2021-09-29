package lexer

import (
	"fmt"
	"os"
	"strings"
	"text/tabwriter"
	"unicode"

	"github.com/logrusorgru/aurora"
)

// Tokenize Tokenizes the input file
func Tokenize(file *os.File, showTokens bool) []Token {
	reader := newTokenReader(file)
	tokens := make([]Token, 0)

	w := tabwriter.NewWriter(os.Stdout, 1, 1, 2, ' ', 0)
	var oldTok *Token
	for tok := readToken(&reader); reader.EOF == false; tok = readToken(&reader) {
		if showTokens {
			fmt.Fprintf(w, "./%s:%d:%d\t%s\t%s\n", reader.File, tok.Line, tok.Index, tok.ID, tok.Data)
		}

		if oldTok == nil || !(tok.ID == EOL && oldTok.ID == EOL) {
			tokens = append(tokens, *tok)
			oldTok = tok
		}
	}
	w.Flush()

	tokens = append(tokens, Token{ID: EOF, Data: "EOF", Line: reader.Line, Index: reader.Index})

	return tokens
}

func readToken(reader *sourceCodeReader) *Token {
	tok := Token{Line: reader.Line, Index: reader.Index}

	peek := reader.peekRune()

	if reader.EOF {
		tok.ID = EOF
		tok.Data = "EOF"
		return &tok
	}

	// Skip all whitespace
	if unicode.IsSpace(peek) {
		for unicode.IsSpace(reader.peekRune()) {
			c := reader.readChar()
			if c == '\n' {
				tok.ID = EOL
				tok.Data = "EOL"
				return &tok
			}
		}

		peek = reader.peekRune()
	}

	// Comment
	if peek == '/' {
		reader.readChar()
		if reader.peekRune() == '/' {
			reader.readLine()
			return readToken(reader)
		}

		logSyntaxError(reader, "Malformed comment")
	}

	// String constants TODO: added error for no closing "
	if peek == '"' {
		data := string(reader.readChar())
		for reader.peekRune() != '"' {
			data += string(reader.readChar())
		}
		data += string(reader.readChar())

		tok.ID = Literal
		tok.Data = data
		return &tok
	}

	// Char constants TODO: added error for no closing '
	if peek == '\'' {
		data := string(reader.readChar())
		for reader.peekRune() != '\'' {
			data += string(reader.readChar())
		}
		data += string(reader.readChar())

		tok.ID = Literal
		tok.Data = data
		return &tok
	}

	// Number constants TODO: Added floats and errors for to many . etc
	if unicode.IsDigit(peek) {
		data := string(reader.readChar())
		for unicode.IsDigit(reader.peekRune()) {
			data += string(reader.readChar())
		}

		tok.ID = Literal
		tok.Data = data
		return &tok
	}

	// Words
	if isIdentifierStart(peek) {
		data := string(reader.readChar())

		for r := reader.peekRune(); unicode.IsLetter(r) || unicode.IsNumber(r) || r == '_'; r = reader.peekRune() {
			data += string(reader.readChar())
		}

		// Keyword
		b, i := isKeyword(data)
		if b {
			tok.ID = Keyword
			tok.Data = KeywordType(i)
			return &tok
		}

		// Boolean literal/constant
		if strings.ToLower(data) == "true" || strings.ToLower(data) == "false" {
			tok.ID = Literal
			tok.Data = strings.ToLower(data) == "true"
			return &tok
		}

		// Identifier
		tok.ID = Identifier
		tok.Data = data
		return &tok
	}

	// Operator
	if isOperator(peek) {
		tok.ID = Operator
		tok.Data = string(reader.readChar())
		return &tok
	}

	// Bracket
	if isBracket(peek) {
		tok.ID = Bracket
		tok.Data = string(reader.readChar())
		return &tok
	}

	logSyntaxError(reader, "Unhandled rune! ")
	return nil
}

func isOperator(r rune) bool {
	return r == '.' || r == ',' || // Misc
		r == '+' || r == '-' || r == '*' || r == '/' || r == '%' || // Math
		r == '|' || r == '&' || r == '<' || r == '>' || // Comparison
		r == '=' // Assignment
}

func isBracket(r rune) bool {
	return r == '{' || r == '}' || // Block
		r == '(' || r == ')' || // Call
		r == '[' || r == ']' // Array
}

func isKeyword(identifier string) (bool, int) {
	for i, keyword := range keywords {
		if strings.ToLower(identifier) == strings.ToLower(keyword) {
			return true, i
		}
	}
	return false, -1
}

func isIdentifierStart(r rune) bool {
	return unicode.IsLetter(r) || r == '_'
}

func isIdentifierRest(r rune) bool {
	return unicode.IsLetter(r) || unicode.IsDigit(r) || r == '_'
}

func logSyntaxError(reader *sourceCodeReader, message string) {
	msg := fmt.Sprintf("./%s:%d:%d ", reader.File, reader.Line, reader.Index)
	reader.readChar()
	logError(msg + reader.CurrentLine)

	indicator := ""
	for i := 0; i < len(msg)+reader.Index-2; i++ {
		indicator += " "
	}
	indicator += "^"

	logError(indicator + " " + message)

	os.Exit(1)
}

func logError(message string) {
	fmt.Println(aurora.Red(message))
}
