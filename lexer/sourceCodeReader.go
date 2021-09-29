package lexer

import (
	"bufio"
	"io"
	"os"
)

func newTokenReader(file *os.File) sourceCodeReader {
	fi, _ := file.Stat()
	return sourceCodeReader{
		buf:   *bufio.NewReader(file),
		File:  fi.Name(),
		Line:  1,
		Index: 1,
	}
}

func (reader *sourceCodeReader) readChar() rune {
	r, _, err := reader.buf.ReadRune()

	if err == io.EOF {
		reader.EOF = true
		return ' '
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

func (reader *sourceCodeReader) readLine() string {
	result := ""
	for reader.peekRune() != '\n' && reader.peekRune() != '\r' {
		result += string(reader.readChar())
	}
	return result
}

func (reader *sourceCodeReader) peekRune() rune {
	r, err := reader.buf.Peek(1)

	if err != nil {
		reader.EOF = true
		return ' '
	}

	return rune(r[0])
}
