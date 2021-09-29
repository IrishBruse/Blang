package parser

import (
	"github.com/IrishBruse/iblang/lexer"
)

type astNode struct {
	token lexer.Token
}

type binNode struct {
	astNode
	left  astNode
	right astNode
}

type programNode struct {
	children []astNode
}

type intNode struct {
	astNode
	num int
}

type tokenReader struct {
	tokens       []lexer.Token
	currentToken int
}

func (t *tokenReader) Next() lexer.Token {
	tok := t.tokens[t.currentToken]
	t.currentToken++
	return tok
}

func (t tokenReader) Peek() lexer.Token {
	tok := t.tokens[t.currentToken]
	return tok
}

func (t tokenReader) EOF() bool {
	return t.currentToken >= len(t.tokens)
}
