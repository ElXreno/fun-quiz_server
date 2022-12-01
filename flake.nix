{
  description = "Flake providing FunQuiz API Server.";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let pkgs = nixpkgs.legacyPackages.${system}; in
      rec {
        packages =
          let fun-quiz-api = with pkgs;
          buildDotnetModule rec {
            pname = "fun-quiz_server";
            version = "0.1.0";

            src = self;

            projectFile = "API/API.csproj";
            nugetDeps = ./nuget-deps.nix;

            dotnet-sdk = dotnet-sdk_6;
            dotnet-runtime = dotnet-aspnetcore_6;

            nativeBuildInputs = [ autoPatchelfHook ];

            buildInputs = [ stdenv.cc.cc.lib ];

            meta = with lib; {
              description = "Backend for FunQuiz";
              maintainers = with maintainers; [ elxreno ];
              platforms = platforms.linux;
            };
          };
          in
          flake-utils.lib.flattenTree {
            fun-quiz-api = fun-quiz-api;
            default = fun-quiz-api;
          };
        apps.fun-quiz-api = flake-utils.lib.mkApp { drv = packages.fun-quiz-api; exePath = "/bin/API"; };
        apps.default = apps.fun-quiz-api;

        nixosModules.default = { config, lib, pkgs, ... }: with lib;
          let cfg = config.services.fun-quiz-api;
          in
          {
            options.services.fun-quiz-api = {
              enable = mkEnableOption "FunQuiz API Server";
              port = mkOption {
                type = types.int;
                default = 9090;
                example = 5000;
                description = "Port to listen on.";
              };
              databaseConnectionUrl = mkOption {
                type = types.str;
                default = "";
                example = "Host=localhost;Port=5432;Database=funquiz;Username=funquiz;Password=funquiz";
                description = "Database connection string.";
              };
              databaseConnectionUrlFile = mkOption {
                type = types.nullOr types.path;
                default = null;
                example = "/run/secrets/fun-quiz_database_connection_string";
                description = "Database connection string file.";
              };
              jwt = {
                audience = mkOption {
                  type = types.str;
                  default = "FunQuiz";
                  example = "fun-quiz";
                  description = "JWT audience.";
                };
                issuer = mkOption {
                  type = types.str;
                  default = "FunQuiz";
                  example = "fun-quiz";
                  description = "JWT issuer (domain).";
                };
                secret = mkOption {
                  type = types.str;
                  default = "";
                  example = "7MczIPQQ9qAJihmMKYFUJJ0JSRoHryUrBA2re4K9OCTZJhSJ";
                  description = "JWT secret.";
                };
                secretFile = mkOption {
                  type = types.nullOr types.path;
                  default = null;
                  example = "/run/secrets/fun-quiz_jwt_secret";
                  description = "JWT secret file.";
                };
                tokenExpiration = mkOption {
                  type = types.int;
                  default = 60;
                  example = 60;
                  description = "JWT token expiration time in minutes.";
                };
              };
              user = mkOption {
                type = types.str;
                default = "nobody";
                description = "User account under which FunQuiz API Server runs.";
              };
              group = mkOption {
                type = types.str;
                default = "nogroup";
                description = "Group account under which FunQuiz API Server runs.";
              };
              package = mkOption {
                type = types.package;
                default = pkgs.fun-quiz-api;
                description = "FunQuiz API Server package to use.";
              };
            };

            config =
              mkIf cfg.enable {
                assertions = [
                  {
                    assertion = cfg.databaseConnectionUrl != "" || cfg.databaseConnectionUrlFile != null;
                    message = "Either services.fun-quiz-api.databaseConnectionUrl or services.fun-quiz-api.databaseConnectionUrlFile must be set.";
                  }

                  {
                    assertion = cfg.jwt.secret != "" || cfg.jwt.secretFile != null;
                    message = "Either services.fun-quiz-api.jwt.secret or services.fun-quiz-api.jwt.secretFile must be set.";
                  }
                ];

                systemd.services.fun-quiz-api =
                  let
                    databaseConnectionUrlFile =
                      if cfg.databaseConnectionUrlFile != null
                      then cfg.databaseConnectionUrlFile
                      else pkgs.writeText "fun-quiz_connection_string" cfg.databaseConnectionUrl;

                    jwtSecretFile =
                      if cfg.jwt.secretFile != null
                      then cfg.jwt.secretFile
                      else pkgs.writeText "fun-quiz_jwt_secret" cfg.jwt.secret;
                  in
                  {
                    description = "FunQuiz API Server";
                    wantedBy = [ "multi-user.target" ];
                    after = [ "postgresql.service" ];
                    requires = [ "postgresql.service" ];
                    script = ''
                      export ASPNETCORE_URLS="http://localhost:${toString cfg.port}"
                      export ASPNETCORE_ENVIRONMENT="Production"
                      export ASPNETCORE_ConnectionStrings__DefaultConnection="${"$(cat ${databaseConnectionUrlFile})"}"

                      export ASPNETCORE_JWT__ValidAudience="${cfg.jwt.audience}"
                      export ASPNETCORE_JWT__ValidIssuer="${cfg.jwt.issuer}"
                      export ASPNETCORE_JWT__Secret="${"$(cat ${jwtSecretFile})"}"
                      export ASPNETCORE_JWT__TokenExpirationInMinutes="${toString cfg.jwt.tokenExpiration}"
                      ${cfg.package}/bin/API
                    '';
                    serviceConfig = {
                      Type = "simple";
                      Restart = "always";
                      RestartSec = 10;
                      User = cfg.user;
                      Group = cfg.group;
                      WorkingDirectory = cfg.package;
                    };
                  };
              };
          };
      }
    );
}
