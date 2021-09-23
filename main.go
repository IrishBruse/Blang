package main

import (
	"fmt"
	"log"
	"os"
)

func main() {
	b, err := os.ReadFile("./example.ec")

	if err != nil {
		log.Fatal(err)
	}

	fileText := string(b[:])

	program := Tokenize(fileText)

	fmt.Println()

	for _, token := range program.tokens {
		fmt.Println(token)
	}
}
