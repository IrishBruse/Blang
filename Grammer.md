# B Language Grammar

## Legend

| Notation | Meaning                             |
| :------- | :---------------------------------- |
| `foo?`   | Zero or one occurrence of `foo`.    |
| `foo*`   | Zero or more occurrences of `foo`.  |
| `foo+`   | One or more occurrences of `foo`.   |
| `'text'` | Represents the literal text `text`. |

## Lexical Conventions

-   **Identifiers**: Start with an `<alpha>` character and can be followed by up to seven `<alpha_digit>` characters (for a total maximum length of 8).
-   **Keywords**: All keywords (`auto`, `extrn`, `if`, `else`, etc.) are reserved and must be in lowercase.
-   **Character Set**: The B language uses the ANSCII character set.
    -   `<alpha>`: Any uppercase letter (`A-Z`), lowercase letter (`a-z`), or underscore (`_`).
    -   `<digit>`: Any digit from `0` to `9`.
    -   `<alpha_digit>`: An `<alpha>` or a `<digit>`.
    -   `<char>`: Any character. Special characters can be represented with an escape sequence starting with `*`, such as `*n` (new line), `*t` (tab), `*0` (null), and `*e` (end-of-file).

---

## Grammar

```groovy
CompilationUnit
    Definition*

Definition
    GlobalVariableDecleration
    FunctionDecleration

GlobalVariableDecleration
    (Identifier, ('[', Constant?, ']')?, (Ival, (',', Ival)*)?, ';')

FunctionDecleration
    (Identifier, '(', (Identifier, (',', Identifier)*)?, ')', Block)

Block
    ('{', Statement, '}')
    Statement

Statement
    AutoStatement
    ExternStatement
    CaseStatement
    IfStatement
    WhileStatement
    SwitchStatement
    GotoStatement
    ReturnStatement
    LabelStatement
    (Rvalue?, ';')

AutoStatement
    ('auto', Identifier, Constant?, (',', Identifier, Constant?)*, ';')

ExternStatement
    ('extrn', Identifier, (',', Identifier)*, ';')

CaseStatement
    ('case', Constant, ':')

IfStatement
    ('if', '(', Rvalue, ')', Block, ('else', Block)?)

WhileStatement
    ('while', '(', Rvalue, ')', Block)

SwitchStatement
    ('switch', Rvalue, Block)

GotoStatement
    ('goto', Rvalue, ';')

ReturnStatement
    ('return', ('(', Rvalue, ')')?, ';')

LabelStatement
    (Identifier, ':')


<!-- WIP -->

Rvalue
    ('(', Rvalue, ')')
    Lvalue
    Constant
    (Lvalue, Assign, Rvalue)
    (IncDec, Lvalue)
    (Lvalue, IncDec)
    (Unary, Rvalue)
    ('&', Lvalue)
    (Rvalue, Binary, Rvalue)
    (Rvalue, '?', Rvalue, ':', Rvalue)
    (Rvalue, '(', (Rvalue, (',', Rvalue)* )?, ')')

Assign
    '=', Binary?

IncDec
    '++'
    '--'

Unary
    '-'
    '!'

Binary
    '|' | '&'
    '==' | '!='
    '<' | '<=' | '>' | '>='
    '<<' | '>>'
    '-' | '+'
    '%' | '*' | '/'

Lvalue
    Identifier
    ('*', Rvalue)
    (Rvalue, '[', Rvalue, ']')

Ival
    Constant
    Identifier

Constant
    <digit>+
    ('\'', <char>[1-2], '\'')
    ('"', <char>*, '"')

Identifier
    <alpha>, (<alpha> | <digit>)[0-7]
```
