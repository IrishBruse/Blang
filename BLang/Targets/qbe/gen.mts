import instructions from "./qbeInstructions.ts";
import { writeFileSync } from "node:fs";

process.chdir("BLang/Targets/qbe");

let text = "";
text += "namespace BLang.Targets.qbe;\n";
text += "\n";
text += "public partial class QbeOutput\n";
text += "{\n";

for (const group of Object.keys(instructions)) {
    const groupName = PascalCase(group);
    GenGroup(groupName, instructions[group]);
}

function GenGroup(groupName: string, group: any) {
    text += `    // ${groupName}\n`;
    text += `\n`;
    for (const instr of Object.keys(group)) {
        const instruction = group[instr];
        const name = PascalCase(instr);

        const args = Object.entries(instruction?.args ?? []);

        const parameters = args
            .map(([name, type]) => {
                return type + " " + name;
            })
            .join(", ");

        const variables = args
            .map(([name, type]) => {
                return `{${name}}`;
            })
            .join(", ");

        text += `    /// <summary> ${instruction.description} </summary>\n`;
        text += `    public void ${name}(${parameters})\n`;
        text += `    {\n`;
        text += `        Write($"${instr} ${variables}");\n`;
        text += `    }\n`;
        text += `\n`;
    }
}

text += "}\n";

writeFileSync("./QbeOutput.gen.cs", text);

function PascalCase(text: string): string {
    return text[0].toUpperCase() + text.slice(1);
}
