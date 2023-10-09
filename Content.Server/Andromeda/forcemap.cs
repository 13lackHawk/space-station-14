using Robust.Shared.Console;
using Content.Server.RoundEnd;

namespace Content.Server.Andromeda.Forcemap
{
    public class forcemap


[Dependency] private readonly IConsoleHost.ExecuteCommand() = default!;
[Dependency] private readonly OnRoundEnd  = default!;


     

        public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundEnd);
    }
     {
                      shell.ExecuteCommand($"forcemap "" ");

      }

}



