package walker

import (
	"fmt"
	"os"

	"github.com/IrishBruse/iblang/parser"
)

// WalkNasm walks the ast and converts it to nasm assembly
func WalkNasm(node interface{}, walker parser.Walker) {

	for i := 1; i < walker.Depth; i++ {
		fmt.Print("    ")
	}

	switch n := node.(type) {
	case parser.StringNode:
		fmt.Printf("%T - %+v\n", n, n.Value)
	case parser.BlockNode:
		fmt.Printf("%T - \n", n)
	case parser.FuncDefNode:
		writeNasmLabel(walker.SrcFile, n.Identifier)
		walker.SrcFile.WriteString("    mov ecx, 0\n")
		walker.SrcFile.WriteString("    call  ExitProcess\n")

		fmt.Printf("%T - %s: %v\n", n, n.Identifier, n.Params)
	case parser.FuncCallNode:
		fmt.Printf("%T - %s: %v\n", n, n.Identifier, n.Params)
	case parser.Ast:
		writeNasmExtern(walker.SrcFile, "GetStdHandle")
		writeNasmExtern(walker.SrcFile, "WriteConsoleA")
		writeNasmExtern(walker.SrcFile, "ExitProcess")

		walker.SrcFile.WriteString("\n")

		walker.SrcFile.WriteString("global main   ; Export the entry point\n")
		walker.SrcFile.WriteString("section .text\n")
		fmt.Printf("%T - Root\n", n)

	default:
		fmt.Printf("%T - Unknown!\n", n)
	}
}

func writeNasmLabel(f *os.File, label string) {
	f.WriteString(label + ":\n")
}

func writeNasmExtern(f *os.File, extern string) {
	f.WriteString("extern " + extern + "\n")
}
