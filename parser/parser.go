package parser

import (
	"github.com/IrishBruse/iblang/lexer"
)

// Parse parses the tokens from the lexer and creates an AST out of it
func Parse(tokens []lexer.Token, printAst bool) Ast {
	// reader := tokenReader{tokens: tokens}

	// ast := Ast{
	// 	nodes: []nodeVisitor{
	// 		namespaceNode{identifier: "foo"},
	// 		importNode{identifier: "system"},
	// 		funcDefNode{
	// 			identifier: "main",
	// 			params: []varNode{{
	// 				identifier: "args",
	// 				varType:    "string[]",
	// 			}},
	// 		},
	// 		BlockNode{
	// 			children: []nodeVisitor{
	// 				funcCallNode{
	// 					identifier: "print",
	// 					params: []nodeVisitor{
	// 						StringNode{Value: "Hello, World"},
	// 					},
	// 				},
	// 				ifNode{
	// 					cond: boolNode{value: true},
	// 					block: BlockNode{
	// 						children: []nodeVisitor{
	// 							funcCallNode{
	// 								identifier: "print",
	// 								params: []nodeVisitor{
	// 									StringNode{Value: "Hello, World"},
	// 								},
	// 							},
	// 						},
	// 					}},
	// 				funcCallNode{
	// 					identifier: "assert",
	// 					params: []nodeVisitor{
	// 						boolNode{value: true},
	// 						StringNode{Value: "Assert called!"},
	// 					},
	// 				},
	// 			},
	// 		},
	// 	},
	// }

	ast := Ast{
		nodes: []nodeVisitor{
			FuncDefNode{
				Identifier: "main",
				Params:     []varNode{},
				block: BlockNode{
					children: []nodeVisitor{
						FuncCallNode{
							Identifier: "print",
							Params: []nodeVisitor{
								StringNode{Value: "Hello, World!"},
							},
						},
					},
				},
			},
		},
	}

	return ast
}
