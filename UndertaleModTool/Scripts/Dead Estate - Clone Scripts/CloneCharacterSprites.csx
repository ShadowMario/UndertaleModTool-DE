using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using UndertaleModLib.Util;

string[] characterList = new string[] {
	"Jules",
	"Jeff - NOT FUNCTIONAL YET",
	"Cordelia - NOT FUNCTIONAL YET",
	"Fuji - NOT FUNCTIONAL YET",
	"Boss - NOT FUNCTIONAL YET",
	"Luis - NOT FUNCTIONAL YET",
	"Mumba - NOT FUNCTIONAL YET",
	"Lydia - NOT FUNCTIONAL YET",
	"Digby - NOT FUNCTIONAL YET",
	"",
	"Axel cannot be cloned due to him being built different unironically"
};

UndertaleEmbeddedTexture embed = null;
string prefix = "";
string character = "";
EnsureDataLoaded();
thing();
if(prefix == "") return;

string clonePrefix = ScriptInputDialog(ScriptPath, "Insert your Clone's Name", "PlayerCopy", "Cancel", "Clone", true, false);
if(clonePrefix == null) return;

if(clonePrefix == "")
{
	ScriptError("Clone name can't be empty!!");
	return;
}

TextureWorker worker = new TextureWorker();
if(File.Exists(Path.Combine(Path.GetDirectoryName(ScriptPath), "CloneImages") + "\\" + character + ".txt"))
{
	clonePrefix = "spr" + clonePrefix.Replace(" ", "_");
	cloneEmbeddedTexture();
	if(embed == null) return;
	
	cloneSprites();
	ScriptMessage("Character Cloned!\nNew Sprites (starts with): " + clonePrefix);
}
else
{
	ScriptError("The Character prefix you've put is incorrect");
}

UndertaleSprite[] clonedSprites = new UndertaleSprite[10000];
UndertaleTexturePageItem[] clonedItems = new UndertaleTexturePageItem[10000];
void cloneSprites()
{
	int sprites = 0;
	int items = 0;
	string line = "";
	UndertaleSprite newSprite = null;
	using (StreamReader reader = new StreamReader(Path.Combine(Path.GetDirectoryName(ScriptPath), "CloneImages") + "\\" + character + ".txt"))
	{
		while ((line = reader.ReadLine()) != null)
		{
			line = line.Trim();
			string[] split = line.Split(" ");
			for (int i = 0 ; i < split.Length ; i++)
			{
				split[i] = split[i].Trim();
			}

			switch(split.Length)
			{
				case 9:
					newSprite = new UndertaleSprite();
					newSprite.Name = new UndertaleString(clonePrefix + split[0]);
					newSprite.Width = uint.Parse(split[1]);
					newSprite.Height = uint.Parse(split[2]);
					newSprite.MarginLeft = int.Parse(split[3]);
					newSprite.MarginRight = int.Parse(split[4]);
					newSprite.MarginTop = int.Parse(split[5]);
					newSprite.MarginBottom = int.Parse(split[6]);
					newSprite.OriginX = int.Parse(split[7]);
					newSprite.OriginY = int.Parse(split[8]);
					Data.Sprites.Add(newSprite);
					sprites++;
					//Console.WriteLine("New sprite #" + sprites + ": " + line);
					break;
				case 10:
					UndertaleTexturePageItem newItem = new UndertaleTexturePageItem();
					newItem.Name = new UndertaleString("PageItem " + Data.TexturePageItems.Count);
					newItem.SourceX = ushort.Parse(split[0]);
					newItem.SourceY = ushort.Parse(split[1]);
					newItem.SourceWidth = ushort.Parse(split[2]);
					newItem.SourceHeight = ushort.Parse(split[3]);
					newItem.TargetX = ushort.Parse(split[4]);
					newItem.TargetY = ushort.Parse(split[5]);
					newItem.TargetWidth = ushort.Parse(split[6]);
					newItem.TargetHeight = ushort.Parse(split[7]);
					newItem.BoundingWidth = ushort.Parse(split[8]);
					newItem.BoundingHeight = ushort.Parse(split[9]);
					newItem.TexturePage = embed;
					Data.TexturePageItems.Add(newItem);
					
					UndertaleSprite.TextureEntry texentry = new UndertaleSprite.TextureEntry();
					texentry.Texture = newItem;
					newSprite.Textures.Add(texentry);
					items++;
					//Console.WriteLine("New frame #" + items + ": " + line);
					break;
			}
		}
	}
}

void cloneEmbeddedTexture()
{
	string imgPath = Path.Combine(Path.GetDirectoryName(ScriptPath), "CloneImages") + "\\" + character + ".png";
	try
	{
		embed = new UndertaleEmbeddedTexture();
		embed.Name = new UndertaleString("Texture " + Data.EmbeddedTextures.Count);
		embed.TextureData.TextureBlob = File.ReadAllBytes(imgPath);
		embed.Scaled = 1;
		Data.EmbeddedTextures.Add(embed);
	}
	catch(Exception ex)
	{
		//embed failure!! Laugh at this user
		ScriptMessage("Failed to import file \"" + imgPath + "\" due to: " + ex.Message);
	}
}

void thing(bool isHelp = false)
{
	string resultStr = "";
	if (!isHelp) resultStr = ScriptInputDialog(ScriptPath, "Insert a Character Name (\"help\" to show list):", "Jules", "Cancel", "Select", true, false);
	else resultStr = SimpleTextInput(ScriptPath, "List of Sprites:", string.Join("\n", characterList), true);

	if(!isHelp)
	{
		if(resultStr != null) //Didn't cancel
		{
			// Put in player sprite prefix
			if(resultStr.ToLower() != "help")
			{
				character = resultStr.ToLower().Trim();
				switch(character)
				{
					case "jeff":
						prefix = "sprTrucker";
						break;
					case "jules":
						prefix = "sprPlayer";
						break;
					case "cordelia":
						prefix = "sprWitch";
						break;
					case "fuji":
						prefix = "sprFuji";
						break;
					case "boss":
						prefix = "sprBoss";
						break;
					case "luis":
						prefix = "sprLuis";
						break;
					case "mumba":
						prefix = "sprMumba";
						break;
					case "lydia":
						prefix = "sprLydia";
						break;
					case "digby":
						prefix = "sprDigby";
						break;
					default:
						ScriptError("Invalid Character!");
						return;
				}
			}
			// Put in "help"
			else thing(true);
		}
		else return;
	}
	else thing(); //Closed help list, open again
}