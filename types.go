package main

type TokenType int

const (
	Keyword TokenType = iota
	Float             // TODO floating point math ints for now
	Integer
	String
	Char
	BlockOpen
	BlockClose
)

type Token struct {
	id    TokenType
	data  string
	line  int
	index int
}

type Program struct {
	tokens []Token
}
