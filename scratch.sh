mkdir -p ./scratch/bin
mkdir -p ./scratch/obj

qbe ./scratch/scratch.ssa > ./scratch/obj/qbe-scratch.s
gcc ./scratch/obj/qbe-scratch.s -o ./scratch/bin/qbe-scratch
/tmp/qbe-scratch
