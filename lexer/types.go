package lexer

import "bufio"

// TokenType is the catagory of token identified eg Keyword(print), Identifier(foo) or Literal(123) etc...
type TokenType int

// TokenType
const (
	Keyword TokenType = iota
	Identifier
	Literal
	Operator
	Bracket
	EOL
	EOF
)

// Debug so it will print correctly
var tokens = []string{"Keyword", "Identifier", "Literal", "Operator", "Bracket", "EOL", "EOF"}

func (t TokenType) String() string {
	return tokens[t]
}

// TokenData is either a string, rune or TokenType
type TokenData interface {
}

// KeywordType is just an enum for keywords
type KeywordType int

// KeywordType
const (
	Print KeywordType = iota // Keyword "functions"
	Assert
	If // Control Flow
	For
	While
	Return // Exit Stuff
	Continue
	Break
	Func // Function decleration
	Namespace
)

// Debug so it will print correctly
var keywords = []string{"Print", "Assert", "If", "For", "While", "Return", "Continue", "Break", "Func", "Namespace"}

func (t KeywordType) String() string {
	return keywords[t]
}

// Token is an Identified piece of the source code
type Token struct {
	ID    TokenType
	Data  TokenData
	Line  int
	Index int
}

type sourceCodeReader struct {
	File        string
	EOF         bool
	Line        int
	Index       int
	CurrentLine string
	buf         bufio.Reader
}
