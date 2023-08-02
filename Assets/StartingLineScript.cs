using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using KModkit;
using UnityEngine.UI;

using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
public class StartingLineScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule Modulo;
    public KMBombInfo Bomb;
    public KMSelectable[] buttons;
    public Text text;
    public TextMesh[] ButtonTxt;
    public Transform wow;
    public MeshRenderer[] LEDs;
    public TextAsset JSON;
    public bool TestMode;

    static private int _moduleIdCounter = 1, gone = 1;
    static private bool Ret2Go = false;
    private int _moduleId, here;
    private string startingLine;
    private string NameOfSelectedMod;
    private int countdone;
    private string Alpha = "EEEEEEEEEEEEAAAAAAAAAIIIIIIIIIOOOOOOOONNNNNNRRRRRRTTTTTTLLLLSSSSUUUUDDDDGGGBBCCMMPPFFHHVVWWYYKJXQZ";
    private int[] buttonnums = new int[4];
    private char[] keylets = new char[4];
    private bool[] pressed = new bool[4] { false, false, false, false };
    private int stage;
    private static readonly string[] SolveTexts = { "Ya did.", "Boing!", "Congrats!", "This is the story of a girl...", "It's been one week since you've looked at me...", "Somebody once told me...", "The world is a vampire...", "Uno, dos, one two tres quatro!...", "Can't touch this...", "I've got another confession to make...", "We know where we're going...", "Everybody dance now...", "Reluctantly crouched at �����...", "This is how we do it...", "One, two, three, uh...", "Did you know that in terms of manual edge cases, The Radio has one of the worst?- wait. wrong paper.", "I hope they weren't as bad as something like \"Batteries\"...", "Whey! Nice job champ.", "lesgo", "Oh, come on, those were easy ones...", "Did you know? This module used to ping the repo 1800 times. Needless to say, Timwi wasn't happy.", "Also try Flavor Text!", "Also try Flavor Text EX!", "did you know that i draw? now you do.", "Did you know that you are an amazing person? Have a great day! <3", "I hope you have a wonderful day you beautiful person!", "Awesome.", "I'm so sick of this SONG dude...", "You're walking into a room. There's nobody around and your phone is dead. Out of the corner of you eye you spot it- the bomb.", "Challenge: If you have Aphantasia, can you imagine a red star for me?", "FALL GUYS\nAMONG US\n\n-Lootcrates", "Jesús Koch placed the Super Roamin' Fifth Base in The Oven!"};
    static Dictionary<string, string> RepoModules;

    void Awake()
    {
        wow.GetComponent<MeshRenderer>().enabled = false;
        _moduleId = _moduleIdCounter++;
        here = gone++;
        for (byte i = 0; i < buttons.Length; i++)
        {
            KMSelectable btn = buttons[i];
            btn.OnInteract += delegate
            {
                StartCoroutine(Handlepress(Array.IndexOf(buttons, btn)));
                return false;
            };
        }
        if (RepoModules == null && _moduleId == 1)
        {
            StartCoroutine(FillRepoModules());
        }
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(WaitPatientlyForTheRepoToLoad());
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator Handlepress(int i)
    {
        yield return null;
        if (pressed[i] == false)
        {
            buttons[i].AddInteractionPunch();
            if (
                buttonnums.Where((str, index) => !pressed[index]).Where(x => x < buttonnums[i]).Count() == 0)
            {
                pressed[i] = true;
                LEDs[i].material.color = new Color(0, 256, 0);
                if (pressed.Count(x => x) == 4)
                {
                    stage++;
                    if (stage == 3)
                    {
                        Audio.PlaySoundAtTransform("solve", Modulo.transform);
                        yield return new WaitForSecondsRealtime(6.05f);
						buttons[i].AddInteractionPunch(10f);
                        Modulo.HandlePass();
                        text.text = SolveTexts[Rnd.Range(0, SolveTexts.Length)];
                        Debug.LogFormat("[The Starting Line #{0}]: Correct. Stage passed, and module solved.", _moduleId);
                    }
                    else
                    {
                        Audio.PlaySoundAtTransform("Press" + Rnd.Range(1, 4), buttons[i].transform);
                        yield return new WaitForSeconds(.5f);
                        Debug.LogFormat("[The Starting Line #{0}]: Correct. Stage passed.", _moduleId);
                        ModuleGenerate();

                    }
                }
                else
                {
                    Audio.PlaySoundAtTransform("Press" + Rnd.Range(1, 4), buttons[i].transform);
                }

            }
            else
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, Modulo.transform);
                Debug.LogFormat("[The Starting Line #{0}]: Not quite!", _moduleId);
                LEDs[i].material.color = new Color(256, 0, 0);
                Modulo.HandleStrike();
                for (int j = 0; j < 4; j++)
                {
                    pressed[j] = true;
                }
                yield return new WaitForSeconds(1f);
                ModuleGenerate();
            }
        }
    }
    IEnumerator WaitPatientlyForTheRepoToLoad()
    {
        if (here != 1)
        {
            text.text = "Another instance of the module is loading the lines, please wait.";
        }
        while (!Ret2Go)
        {
            yield return null;
        }
        yield return null;
        ModuleGenerate();
    }

    void ModuleGenerate()
    {
        string ModuleName = "";
        startingLine = null;
        while (RepoModules.Values.Where(x => x.ToString() == startingLine).Count() != 1 || startingLine == "")
        {
            var mod = RepoModules.PickRandom();
            ModuleName = mod.Key;
            startingLine = mod.Value;
        }
        LetterGen(ModuleName);
        Debug.LogFormat("[The Starting Line #{0}]: Selected {1}'s text of \"{2}\".", _moduleId, ModuleName, startingLine);
        Debug.LogFormat("[The Starting Line #{0}]: Displaying letters {1}", _moduleId, keylets.Join(""));
        Debug.Log(keylets.Join("") + " " + buttonnums.Join(" "));
        text.text = startingLine;
    }

    void LetterGen(string ModuleName)
    {
        Alpha = Alpha.ToList().Shuffle().Join("");
        for (int i = 0; i < 4; i++)
        {
            pressed[i] = false;
            keylets[i] = Alpha[i];
            buttonnums[i] = Array.IndexOf(ModuleName.ToUpper().ToCharArray(), keylets[i]);
            if (buttonnums[i] == -1) buttonnums[i] += 9999;
            ButtonTxt[i].text = keylets[i].ToString();
            LEDs[i].material.color = new Color(0, 0, 0);
        }
        var diffChecker = new HashSet<char>();
        bool allDifferent = keylets.All(s => diffChecker.Add(s));
        if ((buttonnums.Where(x => x == 9998).Count() >= 2 && Rnd.Range(0, 6) != 0) || !allDifferent)
        {
            LetterGen(ModuleName);
        }
    }
    IEnumerator LoadingBar()
    {
        wow.GetComponent<MeshRenderer>().enabled = true;
        while (countdone != LocalJson.Count)
        {
            Vector3 targetScale = Vector3.Lerp(new Vector3(0f, .005f, .01f), new Vector3(.1f, .005f, .01f), (float)countdone / (float)LocalJson.Count);
            Vector3 targetPos = Vector3.Lerp(new Vector3(.0712f, .0154f, .0643f), new Vector3(.0212f, .0154f, .0643f), (float)countdone / (float)LocalJson.Count);
            wow.localScale = Vector3.Lerp(wow.localScale, targetScale, Mathf.Pow(.5f, Time.deltaTime));
            wow.localPosition = Vector3.Lerp(wow.localPosition, targetPos, Mathf.Pow(.5f, Time.deltaTime));
            yield return null;
        }

        wow.localScale = Vector3.Lerp(new Vector3(0f, .005f, .01f), new Vector3(.1f, .005f, .01f), 1);
        wow.localPosition = Vector3.Lerp(new Vector3(.0712f, .0154f, .0643f), new Vector3(.0212f, .0154f, .0643f), 1);
    }

    IEnumerator GenerateModule()
    {
        StartCoroutine(LoadingBar());
        countdone = 0;
        int count2 = 0;
        foreach (Module mod in LocalJson) //<-- this no longer pings the repo 1800 times. rejoice.
        {
            count2++;
            StartCoroutine(Element(mod));
            text.text = "Please Wait, Loading Repository... \n" + countdone + "/" + LocalJson.Count;
            while (count2 - countdone > 128)
            {
                yield return null;
                text.text = "Please Wait, Loading Repository... \n" + countdone + "/" + LocalJson.Count;
            }
        }
        while (countdone < LocalJson.Count)
        {
            yield return null;
            text.text = "Please Wait, Loading Repository... \n" + countdone + "/" + LocalJson.Count;
        }
        text.text = "Repository loaded!";
        yield return new WaitForSeconds(1f);
        wow.GetComponent<MeshRenderer>().enabled = false;
        Debug.LogFormat("Finished!");
        Debug.Log(RepoModules.Join(" , "));
        Ret2Go = true;
        gone = 1    ;

    }
    IEnumerator Element(Module i)
    {
        yield return null;
        var mod = i;
        string ModuleName = mod.Name;
        // Debug.Log("Attempting to fetch manual: " + ModuleName);
        switch (ModuleName)
        {
            case "xobekuJ ehT":
                startingLine = "This module consists of three lanes, each with a string of letters, and a vinyl record player.";
                break;
            case "English Entries":
                startingLine = "The module contains a phrase, and 8 pictures from episodes of this playlist here.";
                break;
            case "Count to 69420":
                startingLine = "A Discord channel called #����� will be shown on the module.";
                break;
            case "Stable Time Signatures":
                startingLine = "This module looks identical to �����, but there are a couple of functional differences.";
                break;
            case "ReGrettaBle Relay":
                startingLine = "The module may look familiar to �����,, however, the module has a status light not shaped like a sphere; has no timer LEDs; and is rotated due to poor placement.";
                break;
            case "Custom Keys":
                startingLine = "There will be two keys and a connection code at the top of the module.";
                break;
            case "Duck, Duck, Goose":
                startingLine = "Upon activating, the module will prompt the user to identify if a displayed bird is a duck, a goose, or neither one of the two species by pressing buttons with corresponding labels.";
                break;
            case "Llama, Llama, Alpaca":
                startingLine = "Upon activating, the module will prompt the user to identify if a displayed animal is a llama, an alpaca, or neither one of those two by pressing buttons with corresponding labels. ";
                break;
            case "Simon Simons":
                startingLine = "This module contains 4 Simon Says modules.";
                break;
            case "Scrutiny Squares":
                startingLine = "The screen displays a slightly modified version of one of the squares below.";
                break;

            default:
                startingLine = mod.Line;
                startingLine = Regex.Replace(startingLine.Trim(), @"<.+?>", "");
                Match m = Regex.Match(startingLine, @"(.+?(?:\n|$|\.|!|\?))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                if (!m.Success || startingLine == "[Manual has no starting line]")
                {
                    Debug.LogWarning("Module " + mod.Name + " has no starting line.");
                }
                else
                {


                    startingLine = m.Groups[0].ToString();
                    startingLine = startingLine.Replace('\n', ' ');
                    startingLine = startingLine.Replace('\r', ' ');
                    startingLine = startingLine.Replace("&#172;", "¬");
                    startingLine = startingLine.Replace("&rsquo:", "\'");
                    startingLine = startingLine.Replace("&#x00d7;", "×");
                    startingLine = startingLine.Replace("&trade;", "™");
                    string tempname = Regex.Escape(ModuleName);
                    startingLine = Regex.Replace(startingLine, tempname, Enumerable.Repeat("�", 5).Join(""), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    if (ModuleName.Length >= 4 && ModuleName.Substring(0, 4) == "The ")
                    {
                        startingLine = Regex.Replace(startingLine, tempname.Substring(4, tempname.Length - 4), Enumerable.Repeat("�", 5).Join(""), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }
                    if (ModuleName[ModuleName.Length - 1] == 's')
                    {
                        startingLine = Regex.Replace(startingLine, tempname.Substring(0, tempname.Length - 1), Enumerable.Repeat("�", 5).Join(""), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    }
                    while (startingLine.Contains("  "))
                    {
                        startingLine = Regex.Replace(startingLine, "  ", Enumerable.Repeat("", 1).Join(""), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }


                }
                break;
        }
        DateTime A = DateTime.Now;
        if(A.Day == 1 && A.Month == 4)
        {
            startingLine = startingLine.Replace('�', 'ඞ');
        }
        RepoModules.Add(mod.Name, startingLine);
        //text.text = startingLine;
        countdone++;
    }

    IEnumerator FillRepoModules()
    {
        Debug.LogFormat("Loading the JSON...");
        StartCoroutine(FetchModules());
        while (!_done)
        {
            yield return null;
        }
        Debug.LogFormat("Fetched!");
        RepoModules = new Dictionary<string, string>();
        StartCoroutine(GenerateModule());
    }

    private bool _done;
    [HideInInspector]
    internal static List<string> toLog = new List<string>();
    WWW Fetch = null;
    internal static List<Module> LocalJson = new List<Module>();
    internal static List<Module> RepoJson = new List<Module>();

    class ktaneData
    {
        public List<Dictionary<string, object>> KtaneModules { get; set; }
    }

    IEnumerator FetchModules()
    {
        yield return null;
        Debug.LogFormat("Attempting to load Starting Lines!");
        _done = false;
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "startingline.json")))
        {
            Debug.LogFormat("Oops, local file doesn't exist. Creating...");
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "startingline.json"), JSON.ToString());
        }
        string Json = File.ReadAllText(Path.Combine(Application.persistentDataPath, "startingline.json"));
        LocalJson = JsonConvert.DeserializeObject<List<Module>>(Json);
        Fetch = new WWW("https://ktane.timwi.de/json/startingline");
        Debug.LogFormat("Attempting to reach the Repository for updates...");
        yield return Fetch;
        if (!Application.isEditor || !TestMode)
        { 
            if (Fetch.error == null)
            {
                Debug.Log("JSON successfully fetched!");
                string Fetched = Fetch.text.Substring(16, Fetch.text.Length - 17);
                Debug.Log(Fetched);
                RepoJson = JsonConvert.DeserializeObject<List<Module>>(Fetched);
                if (LocalJson != RepoJson)
                {
                    Debug.Log("Local JSON appears out of date... Updating local json!");
                    File.WriteAllText(Path.Combine(Application.persistentDataPath, "startingline.json"), Fetched);
                    LocalJson = RepoJson;
                }
            }
            else
            {
                Debug.LogFormat("An error has occurred while fetching modules from the repository: {0}. Using current version.", Fetch.error);
            }
    }
        Debug.LogFormat("Name of the last module: {0}", LocalJson[LocalJson.Count - 1].Name);
        _done = true;
    }

    public string Name { get; private set; }
    public string Line { get; private set; }

    private bool cmdIsValid1(string cmd)
    {
        List<char> valids = new List<char> { '1','2','3','4'};
            foreach (char c in cmd)
            {
                if (!valids.Contains(c))
                {
                    return false;
                }
            valids.Remove(c);

            }
            return true;
        
    }

    // 0.55555555f
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} 1234 [Presses the buttons from top to bottom]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string input = command.Join("");
        Debug.Log(input);
        if (Regex.IsMatch(input, @"^\s*[1234]+\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (input.Length > 4)
            {
                yield return "sendtochaterror Too many positions!";
            }
            else if (input.Length == 4)
            {
                if (cmdIsValid1(input))
                {
                    foreach (char c in input)
                    {
                        int temp = int.Parse(c + "");
                        buttons[temp - 1].OnInteract();
                        yield return new WaitForSecondsRealtime(60f/108f);
                    }
                }
                else
                {
                    yield return "sendtochaterror!f One or more of the specified positions '" + input + "' are invalid!";
                }
            }
            else if (input.Length <= 3)
            {
                yield return "sendtochaterror Not enough positions!";
            }
            yield break;
        }
    }
}
public class Module
{
    public string Name;
    public string Line;
}
