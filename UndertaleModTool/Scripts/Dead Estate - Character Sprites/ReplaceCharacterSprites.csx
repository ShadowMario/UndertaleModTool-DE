using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
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
	if (!isHelp) resultStr = ScriptInputDialog(ScriptPath, "Insert a Sprite Prefix (\"help\" to show list):", "Player", "Cancel", "Replace", true, false);
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
if (!Directory.Exists(texFolder))
{
	ScriptError("Folder \"" + prefix + "\" not found!\nrun ExtractCharacterSprites.csx and try again");
	return;
}

string list = "";
int found = 0;
for (int i = 0 ; i < Data.Sprites.Count ; i++)
{
	addSpriteToList(Data.Sprites[i]);
}

bool errored = false;
if(found > 0)
{
	Array.Resize(ref foundSprites, found);
	bool result = ScriptQuestion("Found " + found + " Sprites, do you wish to replace them?");
	if (result)
	{
		SetProgressBar(null, "Sprites", 0, found);
		StartProgressBarUpdater();
		
		await LoadSprites();
		worker.Cleanup();

		await StopProgressBarUpdater();
		HideProgressBar();
		if(!errored) ScriptMessage("Sprites Replaced Successfully:\n" + list);
	}
}
else ScriptError("No Sprites found!\nThe Character prefix you've put might be incorrect");

async Task LoadSprites()
{
	await Task.Run(() => Parallel.ForEach(foundSprites, LoadSprite));
}

void LoadSprite(UndertaleSprite sprite)
{
	if(!errored && sprite != null)
	{
		try 
		{
			for (int i = 0; i < sprite.Textures.Count; i++)
			{
				if (sprite.Textures[i]?.Texture != null)
				{
					string sprFolder = texFolder + sprite.Name.Content.Substring(prefix.Length) + Path.DirectorySeparatorChar + i + ".png";
					Bitmap image = TextureWorker.ReadImageFromFile(sprFolder);
					image.SetResolution(96.0F, 96.0F);
					sprite.Textures[i].Texture.ReplaceTexture(image);
				}
			}
		}
		catch (Exception ex) 
		{
			errored = true;
			ScriptError("Failed to import a image in " + texFolder + sprite.Name.Content.Substring(prefix.Length) + Path.DirectorySeparatorChar + ":" + ex.Message);
			return;
		}
	}
	IncrementProgressParallel();
}

bool addSpriteToList(UndertaleSprite sprite)
{
	for (int i = 0 ; i < directionalSpriteList.Length ; i++)
	{
		for (int j = 0 ; j < directions.Length ; j++)
		{
			if (checkSprite(sprite, directionalSpriteList[i].ToLower() + directions[j].ToLower()))
				return true;
		}
	}
	
	for (int i = 0 ; i < spriteList.Length ; i++)
	{
		if (checkSprite(sprite, spriteList[i].ToLower()))
			return true;
	}
	return false;
}

bool checkSprite(UndertaleSprite sprite, string postfix)
{
	if(sprite != null && sprite.Name.Content.ToLower() == (prefix.ToLower() + postfix) &&
		Directory.Exists(Path.GetDirectoryName(FilePath) + Path.DirectorySeparatorChar + sprite.Name.Content.Remove(prefix.Length) + Path.DirectorySeparatorChar + postfix + Path.DirectorySeparatorChar))
	{
		foundSprites[found] = sprite;
		list += '\n' + sprite.Name.Content;
		found++;
		return true;
	}
	return false;
}