package parser

import (
	"os"

	"github.com/IrishBruse/iblang/lexer"
)

type astNode struct {
	token lexer.Token
}

type namespaceNode struct {
	astNode
	identifier string
}

func (n namespaceNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	walker.Depth--
}

type importNode struct {
	astNode
	identifier string
}

func (n importNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	walker.Depth--
}

type FuncDefNode struct {
	astNode
	Identifier string
	Params     []varNode
	block      BlockNode
}

func (n FuncDefNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	for _, v := range n.Params {
		v.visitNode(walker)
	}
	n.block.visitNode(walker)
	walker.Depth--
}

type BlockNode struct {
	astNode
	children []nodeVisitor
}

func (n BlockNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	for _, v := range n.children {
		v.visitNode(walker)
	}
	walker.Depth--
}

type FuncCallNode struct {
	astNode
	Identifier string
	Params     []nodeVisitor
}

func (n FuncCallNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	for _, v := range n.Params {
		v.visitNode(walker)
	}
	walker.Depth--
}

type varNode struct {
	astNode
	varType    string
	identifier string
}

func (n varNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	walker.Depth--
}

// Keyword nodes
type ifNode struct {
	astNode
	cond  nodeVisitor
	block BlockNode
}

func (n ifNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	n.cond.visitNode(walker)
	for _, v := range n.block.children {
		v.visitNode(walker)
	}
	walker.Depth--
}

type binNode struct {
	astNode
	left  astNode
	right astNode
}

type nodeVisitor interface {
	visitNode(walker Walker)
}

// Ast The programs ast tree
type Ast struct {
	nodes []nodeVisitor
}

// OnVisit is the function called on every node
type OnVisit func(node interface{}, walker Walker)

// WalkTree walk all the nodes in the tree
func (n Ast) WalkTree(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	for _, node := range n.nodes {
		node.visitNode(walker)
	}
	walker.Depth--
}

type identifierNode struct {
	astNode
	identifier string
}

func (n identifierNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	walker.Depth--
}

type intNode struct {
	astNode
	value int
}

func (n intNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	walker.Depth--
}

type StringNode struct {
	astNode
	Value string
}

func (n StringNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	walker.Depth--
}

type boolNode struct {
	astNode
	value bool
}

func (n boolNode) visitNode(walker Walker) {
	walker.Depth++
	walker.VisitFunc(n, walker)
	walker.Depth--
}

type tokenReader struct {
	tokens       []lexer.Token
	currentToken int
}

// Token reader methods
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

// Walker that walks the ast calling visit `VisitFunc`
type Walker struct {
	Depth     int
	VisitFunc OnVisit
	SrcFile   *os.File
}
