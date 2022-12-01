# shell.nix
with import <nixpkgs> {};

mkShell {
  name = "dotnet-env";
  buildInputs = [
    dotnet-sdk_6
  ];
}