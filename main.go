package main

import (
	"flag"
	"log"
	"os"

	"github.com/IrishBruse/iblang/lexer"
)

var printTokensFlag bool
var printAstFlag bool

func init() {
	flag.BoolVar(&printTokensFlag, "PrintTokens", false, "Toggle printing the token stream from the lexer")
	flag.BoolVar(&printAstFlag, "PrintAST", false, "Toggle printing AST from the parser")
	flag.Parse()
}

func main() {
	sourceFile := "./example.ib"
	file, err := os.Open(sourceFile)

	if err != nil {
		log.Fatal(err)
	}

	lexer.Tokenize(file, printTokensFlag)
}
