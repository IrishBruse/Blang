cd "$(dirname "$0")" # cd to file location

mkdir -p ./misc/bin
mkdir -p ./misc/obj

qbe ./misc/scratch.ssa > ./misc/obj/qbe-scratch.s && \
gcc ./misc/obj/qbe-scratch.s -o ./misc/bin/qbe-scratch && \
./misc/bin/qbe-scratch
