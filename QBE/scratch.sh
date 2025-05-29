rm /tmp/qbe-scratch.s
rm /tmp/qbe-scratch
qbe ./scratch.ssa > /tmp/qbe-scratch.s
gcc /tmp/qbe-scratch.s -o /tmp/qbe-scratch
/tmp/qbe-scratch
