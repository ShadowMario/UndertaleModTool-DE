// Modified with the help of Agentalex9
using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

string[] characterList = new string[] {
	"Jeff A: Trucker",
	"Jeff B: Gnome",
	"Jeff C: YoungJeff",
	"",
	"Jules A: Player",
	"Jules B: Ballistic",
	"Jules C: Faye",
	"",
	"Cordelia A: Witch",
	"Cordelia B: Graduate",
	"Cordelia C: Countess",
	"",
	"Fuji A: Fuji",
	"Fuji B: Senator",
	"Fuji C: Champ",
	"",
	"Boss A: Boss",
	"Boss B: Pringle",
	"Boss C: Snake",
	"",
	"Luis A: Luis",
	"Luis B: Stan",
	"Luis C: Kusabi",
	"",
	"Mumba A: Mumba",
	"Mumba B: Monster",
	"Mumba C: BearMumba",
	"",
	"Lydia A: Lydia",
	"Lydia B: JS",
	"Lydia C: GoddessLydia",
	"",
	"Digby A: Digby",
	"Digby B: Funk",
	"Digby C: Mob",
	"",
	"Axel Sprites aren't on data.win (as far as i'm concerned)",
	"",
	"To Use this Script on a Custom Character,",
	"just insert their sprite prefix without the 'spr',",
	"ex: If my character is 'sprCustomChar', set it to CustomChar"
};

string[] directions = new string[] {"South", "Southeast", "East", "Northeast", "North", "Northwest", "West", "Southwest"};
string[] directionalSpriteList = new string[] {"Idle", "Walk", "Land", "Fall", "Jump", "StartJump"};
string[] spriteList = new string[] {"Death", "Corpse"};

string prefix = "";

EnsureDataLoaded();

thing();

void thing(bool isHelp = false)
{
	string resultStr = "";
	if (!isHelp) resultStr = ScriptInputDialog(ScriptPath, "Insert a Sprite Prefix (\"help\" to show list):", "Player", "Cancel", "Extract", true, false);
	else resultStr = SimpleTextInput(ScriptPath, "List of Sprites:", string.Join("\n", characterList), true);

	if(!isHelp)
	{
		if(resultStr != null) //Didn't cancel
		{
			// Put in player sprite prefix
			if(resultStr.ToLower() != "help") prefix = resultStr;
			// Put in "help"
			else thing(true);
		}
		else return;
	}
	else thing(); //Closed help list, open again
}

if(prefix == "")
{
	return;
}

prefix = "spr" + prefix;
TextureWorker worker = new TextureWorker();
string texFolder = Path.GetDirectoryName(FilePath) + Path.DirectorySeparatorChar;

UndertaleSprite[] foundSprites = {};
Array.Resize(ref foundSprites, spriteList.Length + (directionalSpriteList.Length * directions.Length));

texFolder += prefix + Path.DirectorySeparatorChar;
bool folderExists = Directory.Exists(texFolder);

string list = "";
int found = 0;
for (int i = 0 ; i < Data.Sprites.Count ; i++)
{
	addSpriteToList(Data.Sprites[i]);
}

if(found > 0)
{
	if(!folderExists) Directory.CreateDirectory(texFolder);
	Array.Resize(ref foundSprites, found);

	SetProgressBar(null, "Sprites", 0, found);
	StartProgressBarUpdater();

	await DumpSprites();
	worker.Cleanup();

	await StopProgressBarUpdater();
	HideProgressBar();
	ScriptMessage("Sprites Extracted Successfully:\n" + list);
}
else ScriptError("No Sprites found!\nThe Character prefix you've put might be incorrect");

async Task DumpSprites()
{
    await Task.Run(() => Parallel.ForEach(foundSprites, DumpSprite));
}

bool addSpriteToList(UndertaleSprite sprite)
{
	if(sprite == null) return false;

	for (int i = 0 ; i < directionalSpriteList.Length ; i++)
	{
		for (int j = 0 ; j < directions.Length ; j++)
		{
			if(sprite.Name.Content.ToLower() == (prefix.ToLower() + directionalSpriteList[i].ToLower() + directions[j].ToLower()))
			{
				foundSprites[found] = sprite;
				list += '\n' + sprite.Name.Content;
				found++;
				return true;
			}
		}
	}
	
	for (int i = 0 ; i < spriteList.Length ; i++)
	{
		if(sprite.Name.Content.ToLower() == (prefix.ToLower() + spriteList[i].ToLower()))
		{
			foundSprites[found] = sprite;
			list += '\n' + sprite.Name.Content;
			found++;
			return true;
		}
	}
	return false;
}

void DumpSprite(UndertaleSprite sprite)
{
	for (int i = 0; i < sprite.Textures.Count; i++)
	{
		if (sprite.Textures[i]?.Texture != null)
		{
			string sprFolder = texFolder + sprite.Name.Content.Substring(prefix.Length) + Path.DirectorySeparatorChar;
			if(!Directory.Exists(sprFolder)) Directory.CreateDirectory(sprFolder);
			worker.ExportAsPNG(sprite.Textures[i].Texture, sprFolder + i + ".png", null, false);
		}
	}

	IncrementProgressParallel();
}