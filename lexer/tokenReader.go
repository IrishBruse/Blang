package lexer

import (
	"bufio"
	"io"
	"os"
)

func newTokenReader(file *os.File) tokenReader {
	fi, _ := file.Stat()
	return tokenReader{
		buf:   *bufio.NewReader(file),
		File:  fi.Name(),
		Line:  1,
		Index: 1,
	}
}

func (reader *tokenReader) readChar() rune {
	r, _, err := reader.buf.ReadRune()

	if err == io.EOF {
		reader.EOF = true
	}

	reader.Index++
	reader.CurrentLine += string(r)
	if r == '\n' {
		reader.Index = 1
		reader.Line++
		reader.CurrentLine = ""
	}

	return r
}

func (reader *tokenReader) readLine() string {
	result := ""
	for reader.peekRune() != '\n' && reader.peekRune() != '\r' {
		result += string(reader.readChar())
	}
	return result
}

func (reader *tokenReader) peekRune() rune {
	r, err := reader.buf.Peek(1)

	if err != nil {
		reader.EOF = true
		return 't'
	}

	return rune(r[0])
}
