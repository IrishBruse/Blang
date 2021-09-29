package main

import (
	"flag"
	"log"
	"os"

	"github.com/IrishBruse/iblang/lexer"
	"github.com/IrishBruse/iblang/parser"
)

var printTokensFlag bool
var printAstFlag bool

func init() {
	flag.BoolVar(&printTokensFlag, "PrintTokens", false, "Toggle printing the token stream from the lexer")
	flag.BoolVar(&printAstFlag, "PrintAst", false, "Toggle printing AST from the parser")
	flag.Parse()
}

func main() {
	sourceFile := "./example.ib"
	file, err := os.Open(sourceFile)

	if err != nil {
		log.Fatal(err)
	}

	tokens := lexer.Tokenize(file, printTokensFlag)
	parser.Parse(tokens, printAstFlag)
}
