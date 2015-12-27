const String LCD_NAME = "Power Panel";
const String MULTIPLIERS = ".kMGTPEZY";

void Main(string argument)
{
    List<String> text = new List<String>();
    text.Add("Updated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    text.Add("-----------------");
    text.AddRange(Solar_status());

    UpdateLCDs(String.Join("\n", text.ToArray()));
}

List<String> Solar_status() {
    // Find solar panels. Sum their output.
    List<IMyTerminalBlock> solars = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(solars);
    String info = "";

    // Why can't we have a nice API instead?
    System.Text.RegularExpressions.Regex solarRegex = new System.Text.RegularExpressions.Regex(
        "Max Output: (\\d+\\.?\\d*) (\\w?)W.*Current Output: (\\d+\\.?\\d*) (\\w?)W",
        System.Text.RegularExpressions.RegexOptions.Singleline);

    double total_max = 0.0f;
    double total_current = 0.0f;
    int active_solars = 0;

    for(int i = 0;i<solars.Count;i++) {
        info = solars[i].DetailedInfo;
        double currentOutput = 0.0f;
        double maxOutput = 0.0f;
        double parsedDouble;
        System.Text.RegularExpressions.Match match = solarRegex.Match(info);
        if(match.Success) {
            if(Double.TryParse(match.Groups[1].Value, out parsedDouble)) {
                maxOutput = parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
            }
            if(Double.TryParse(match.Groups[3].Value, out parsedDouble)) {
                currentOutput = parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[4].Value));
            }
            if(currentOutput > 0.0) {
                active_solars++;
            }
        }
        total_max += maxOutput;
        total_current += currentOutput;
    }

    List<String> text = new List<String>();
    text.Add("Solar panel status");
    text.Add("Max output: " + Format(total_max) + "W");
    text.Add("Current output: " + Format(total_current) + "W");
    text.Add("Active panels: " + active_solars + " of " + solars.Count);
    text.Add("-----------------");

    return text;
}

void UpdateLCDs(String s) {
    // Find LCDs and update them
    List<IMyTerminalBlock> lcds = new List<IMyTerminalBlock>();
    GridTerminalSystem.SearchBlocksOfName(LCD_NAME, lcds);
    IMyTextPanel panel = null;
    for (int i = 0;i<lcds.Count;i++) {
        if(lcds[i] is IMyTextPanel) {
            panel = (IMyTextPanel)lcds[i];
            panel.WritePublicText(s, false);
            panel.ShowPublicTextOnScreen();
        }
    }
}

String Format(double power) {
    int count = 0;
    while (power > 1000.0) {
        power = power / 1000;
        count++;
    }
    return "" + Math.Round(power,2).ToString("##0.00") + " " + MULTIPLIERS.Substring(count,1);
}
