package parser

import (
	"fmt"

	"github.com/IrishBruse/iblang/lexer"
)

// Parse parses the tokens from the lexer and creates an AST out of it
func Parse(tokens []lexer.Token, printAst bool) {
	reader := tokenReader{tokens: tokens}

	for reader.EOF() == false {
		tok := reader.Next()
		if printAst {
			fmt.Printf("tokens[i]: %v\n", tok)
		}
	}
}
