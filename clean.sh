cd "$(dirname "$0")" # cd to file location

rm -rf Examples/bin
rm -rf Examples/obj

rm -rf Tests/bin
rm -rf Tests/obj

rm -rf scratch/bin
rm -rf scratch/obj

dotnet clean
