// Modified with the help of Agentalex9
using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

string[] characterList = new string[] {
	"Jeff A: sprTrucker",
	"Jeff B: sprGnome",
	"Jeff C: sprYoungJeff",
	"",
	"Jules A: sprPlayer",
	"Jules B: sprBallistic",
	"Jules C: sprFaye",
	"",
	"Cordelia A: sprWitch",
	"Cordelia B: sprGraduate",
	"Cordelia C: sprCountess",
	"",
	"Fuji A: sprFuji",
	"Fuji B: sprSenator",
	"Fuji C: sprChamp",
	"",
	"Boss A: sprBoss",
	"Boss B: sprPringle",
	"Boss C: sprSnake",
	"",
	"Luis A: sprLuis",
	"Luis B: sprStan",
	"Luis C: sprKusabi",
	"",
	"Mumba A: sprMumba",
	"Mumba B: sprMonster",
	"Mumba C: sprBearMumba",
	"",
	"Lydia A: sprLydia",
	"Lydia B: sprJS",
	"Lydia C: sprGoddessLydia",
	"",
	"Digby A: sprDigby",
	"Digby B: sprFunk",
	"Digby C: sprMob",
	"",
	"Axel Sprites aren't on data.win (as far as i'm concerned)"
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
	if (!isHelp) resultStr = ScriptInputDialog(ScriptPath, "Insert a Sprite Prefix (\"help\" to show list):", "sprPlayer", "Cancel", "Extract", true, false);
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

TextureWorker worker = new TextureWorker();
string texFolder = Path.GetDirectoryName(FilePath) + Path.DirectorySeparatorChar;

UndertaleSprite[] foundSprites = {};
Array.Resize(ref foundSprites, spriteList.Length + (directionalSpriteList.Length * directions.Length));

texFolder += prefix + Path.DirectorySeparatorChar;
bool folderExists = Directory.Exists(texFolder);

bool doIt = true;
if (folderExists) doIt = ScriptQuestion("Folder \"" + prefix + "\" already exists, proceeding will overwrite files, are you okay with that?");

if(!doIt) return;

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
	for (int i = 0 ; i < directionalSpriteList.Length ; i++)
	{
		for (int j = 0 ; j < directions.Length ; j++)
		{
			if(sprite != null && sprite.Name.Content.ToLower() == (prefix.ToLower() + directionalSpriteList[i].ToLower() + directions[j].ToLower()))
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
		if(sprite != null && sprite.Name.Content.ToLower() == (prefix.ToLower() + spriteList[i].ToLower()))
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