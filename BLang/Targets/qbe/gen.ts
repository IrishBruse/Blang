import { existsSync, writeFileSync } from "fs";
import { instructions } from "./qbeInstructions.ts";

if (existsSync("BLang/")) {
    process.chdir("BLang/Targets/qbe");
}

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
    for (const mnemonic of Object.keys(group)) {
        const instruction = group[mnemonic];
        const name = instruction.name;

        const args = Object.entries(instruction?.args ?? []);

        let parameters = args
            .map(([name, type]) => `${type} ${name}`)
            .join(", ");

        if (instruction?.ret === "Reg" && !instruction?.retSize) {
            parameters += ", Size regType = Size.L";
        }

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
            writeInstruction(mnemonic, args, instruction);
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

function writeInstruction(mnemonic: string, args: any[], instruction: any) {
    beginLine();
    write("Write(");
    write('$"');

    if (instruction?.retSize) {
        write(`{reg} =${instruction.retSize} `);
    } else if (instruction?.ret === "Reg") {
        write("{reg} ={ToChar(regType)} ");
    }

    write(mnemonic + " ");
    let first = true;
    for (const arg of args) {
        if (!first) {
            write(", ");
        }
        const [name, type] = arg;
        if (type === "FunctionSymbol") {
            write("${" + name + "}");
        } else if (type === "Label") {
            write("@{" + name + "}");
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
