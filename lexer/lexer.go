package lexer

import (
	"fmt"
	"os"
	"unicode"

	"github.com/logrusorgru/aurora"
)

// Tokenize Tokenizes the input file
func Tokenize(file *os.File, showTokens bool) []Token {
	reader := newTokenReader(file)
	tokens := make([]Token, 25)

	for tok := readToken(&reader); reader.EOF == false; tok = readToken(&reader) {
		if showTokens {
			fmt.Printf("./%s:%d:%d\t%s\t%s\n", reader.File, tok.Line, tok.Index, tok.ID, tok.Data)
		}
	}

	return tokens
}

func readToken(reader *tokenReader) *Token {
	// Skip all whitespace
	if unicode.IsSpace(reader.peekRune()) {
		for unicode.IsSpace(reader.peekRune()) {
			reader.readChar()
		}
	}

	peek := reader.peekRune()

	if reader.EOF {
		return nil
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

	tok := Token{Line: reader.Line, Index: reader.Index}

	// String constants
	if peek == '"' {
		data := string(reader.readChar())
		for reader.peekRune() != '"' {
			data += string(reader.readChar())
		}
		data += string(reader.readChar())

		tok.ID = Constant
		tok.Data = data
		return &tok
	}

	// Char constants
	if peek == '\'' {
		data := string(reader.readChar())
		for reader.peekRune() != '\'' {
			data += string(reader.readChar())
		}
		data += string(reader.readChar())

		tok.ID = Constant
		tok.Data = data
		return &tok
	}

	// Number constants TODO: Added floats
	if unicode.IsDigit(peek) {
		data := string(reader.readChar())
		for unicode.IsDigit(reader.peekRune()) {
			data += string(reader.readChar())
		}

		tok.ID = Constant
		tok.Data = data
		return &tok
	}

	// Identifier
	if isIdentifierStart(peek) {
		data := string(reader.readChar())
		for r := reader.peekRune(); unicode.IsLetter(r) || unicode.IsNumber(r) || r == '_'; r = reader.peekRune() {
			data += string(reader.readChar())
		}

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

	logSyntaxError(reader, "Unhandled rune! ")
	return nil
}

func isOperator(r rune) bool {
	return r == '.' || r == ',' || // Misc
		r == '+' || r == '-' || r == '*' || r == '/' || r == '%' || // Math
		r == '|' || r == '&' || r == '<' || r == '>' || // Comparison
		r == '=' || // Assignment
		r == '{' || r == '}' || // Block
		r == '(' || r == ')' || // Call
		r == '[' || r == ']' // Array
}

func isKeyword(identifier string) bool {
	for _, keyword := range keywords {
		if identifier == keyword {
			return true
		}
	}
	return false
}

func isIdentifierStart(r rune) bool {
	return unicode.IsLetter(r) || r == '_'
}

func isIdentifierRest(r rune) bool {
	return unicode.IsLetter(r) || unicode.IsDigit(r) || r == '_'
}

func logSyntaxError(reader *tokenReader, message string) {
	msg := fmt.Sprintf("./%s:%d:%d ", reader.File, reader.Line, reader.Index)
	logError(msg + reader.CurrentLine)

	indicator := ""
	for i := 0; i < len(msg)+reader.Index-1; i++ {
		indicator += "~"
	}
	indicator += "^"

	logError(indicator + " " + message)

	os.Exit(1)
}

func logError(message string) {
	fmt.Println(aurora.Red(message))
}
