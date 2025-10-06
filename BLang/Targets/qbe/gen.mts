import { writeFileSync } from "node:fs";
import instructions from "./qbeInstructions.ts";

process.chdir("BLang/Targets/qbe");

let text = "";
let indentLevel = 0;
const indentString = "    ";

function writeLine(line: string = "") {
    if (line === "") {
        text += "\n";
        return;
    }
    text += indentString.repeat(indentLevel) + line + "\n";
}

function beginLine() {
    text += indentString.repeat(indentLevel);
}
function write(val: string) {
    text += val;
}
function endLine() {
    text += "\n";
}

function indent() {
    indentLevel++;
}

function dedent() {
    indentLevel = Math.max(0, indentLevel - 1);
}

writeLine("namespace BLang.Targets.qbe;");
writeLine();
writeLine("using Val = string;");
writeLine("using Reg = string;");
writeLine("using Label = string;");
writeLine("using FunctionSymbol = string;");
writeLine();
writeLine("public partial class QbeOutput");
writeLine("{");
indent();

for (const group of Object.keys(instructions)) {
    const groupName = PascalCase(group);
    GenGroup(groupName, instructions[group]);
}

function GenGroup(groupName: string, group: any) {
    writeLine(`// ${groupName}`);
    writeLine();
    for (const instr of Object.keys(group)) {
        const instruction = group[instr];
        const name = instruction.name;

        const args = Object.entries(instruction?.args ?? []);

        const parameters = args
            .map(([name, type]) => `${type} ${name}`)
            .join(", ");

        const returnType = instruction?.ret ?? "void";

        writeLine(`/// <summary> ${instruction.description} </summary>`);
        writeLine(`public ${returnType} ${name}(${parameters})`);
        writeLine("{");
        indent();
        if (returnType !== "void") {
            writeLine("Reg reg = GetTempReg();");
        }
        if (instruction?.overrideBody) {
            writeLine(instruction?.overrideBody);
        } else {
            writeInstruction(instr, args, returnType);
        }
        if (returnType !== "void") {
            writeLine("return reg;");
        }
        dedent();
        writeLine("}");
        writeLine();
    }
}

dedent();
writeLine("}");

writeFileSync("./QbeOutput.gen.cs", text);

function writeInstruction(instr: string, args: any[], ret: string) {
    beginLine();
    write("WriteGen(");
    write('$"');

    if (ret === "Reg") {
        write("{reg} =w ");
    }

    write(instr + " ");
    let first = true;
    for (const arg of args) {
        if (!first) {
            write(", ");
        }
        const [name, type] = arg;
        if (type === "FunctionSymbol") {
            write("${" + name + "}");
        } else {
            write("{" + name + "}");
        }

        first = false;
    }
    write('"');
    write(");");
    endLine();
}

function PascalCase(text: string): string {
    return text[0].toUpperCase() + text.slice(1);
}
