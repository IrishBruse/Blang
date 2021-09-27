package lexer

import "bufio"

type tokenReader struct {
	File        string
	EOF         bool
	Line        int
	Index       int
	CurrentLine string
	buf         bufio.Reader
}

// TokenType is the catagory of token identified eg Keyword(print), Identifier(foo) or Constant(123) etc...
type TokenType int

// TokenTypes
const (
	Keyword TokenType = iota
	Identifier
	Constant
	Operator
)

var tokens = []string{"Keyword   ", "Identifier", "Constant  ", "Operator  "}

func (t TokenType) String() string {
	return tokens[t]
}

// TokenData is either a string or a TokenType
type TokenData interface {
}

// KeywordType is just an enum for keywords
type KeywordType int

// Reserved Keywords
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
)

var keywords = []string{"Print", "Assert", "If", "For", "While", "Return", "Continue", "Break", "Func"}

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
