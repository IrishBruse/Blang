cd "$(dirname "$0")" # cd to file location

mkdir -p ./scratch/bin
mkdir -p ./scratch/obj

qbe ./scratch/scratch.ssa > ./scratch/obj/qbe-scratch.s && \
gcc ./scratch/obj/qbe-scratch.s -o ./scratch/bin/qbe-scratch && \
./scratch/bin/qbe-scratch
