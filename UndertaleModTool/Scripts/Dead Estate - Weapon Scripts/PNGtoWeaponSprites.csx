using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using UndertaleModLib.Util;

UndertaleEmbeddedTexture embed = null;
EnsureDataLoaded();

string imgPath = PromptLoadFile(".png", "MagicaVoxel Vox2Png Output File|*.png|All Files|*");
if(imgPath == null) return;

string dimensions = ScriptInputDialog(ScriptPath, "Type MagicaVoxel model dimensions:", "0 0 0", "Cancel", "Import", true, false);
if(dimensions == null) return;

string spriteName = ScriptInputDialog(ScriptPath, "Type Sprite name (ex. sprHandgun, sprMumbaLauncher):", "sprCustomWeapon" + Data.Sprites.Count, "Cancel", "Continue", true, false);
if(spriteName == null) return;

for (int i = 0 ; i < Data.Sprites.Count ; i++)
{
	var sprite = Data.Sprites[i];
	if(sprite?.Name?.Content == spriteName)
	{
		ScriptError("Cannot overwrite " + spriteName + ", use another name instead!");
		return;
	}
}

int originalWidth = 0;
int originalHeight = 0;
cloneEmbeddedTexture();
string[] dimArray = dimensions.Split(" ");

if(dimArray.Length < 3)
{
	ScriptError("Invalid Dimensions! Check out the dimension through MagicaVoxel, copy and paste it.");
	return;
}

int columns = int.Parse(dimArray[0]);
int rows = int.Parse(dimArray[1]);
int layers = int.Parse(dimArray[2]);

TextureWorker worker = new TextureWorker();
if(layers < 1)
{
	ScriptError("Invalid height for this model, stopping script.");
	return;
}

makeSprites();
ScriptMessage("Script finished");

void makeSprites()
{
	UndertaleSprite newSprite = null;
	newSprite = new UndertaleSprite();
	newSprite.Name = new UndertaleString(spriteName);
	newSprite.Width = (uint) columns;
	newSprite.Height = (uint) rows;
	newSprite.MarginLeft = 0;
	newSprite.MarginRight = columns-1;
	newSprite.MarginTop = 0;
	newSprite.MarginBottom = rows-1;
	newSprite.OriginX = (int) Math.Floor((double) columns / 2);
	newSprite.OriginY = (int) Math.Floor((double) rows / 2);
	Data.Sprites.Add(newSprite);

	for (int i = 0 ; i < layers ; i++)
	{
		UndertaleTexturePageItem newItem = new UndertaleTexturePageItem();
		newItem.Name = new UndertaleString("PageItem " + Data.TexturePageItems.Count);
		newItem.SourceX = (ushort) Math.Floor((double) (i * columns) % originalWidth);
		newItem.SourceY = (ushort) (Math.Floor((double) (i * columns) / originalWidth) * rows);
		newItem.SourceWidth = (ushort) columns;
		newItem.SourceHeight = (ushort) rows;
		newItem.TargetX = 0;
		newItem.TargetY = 0;
		newItem.TargetWidth = (ushort) columns;
		newItem.TargetHeight = (ushort) rows;
		newItem.BoundingWidth = (ushort) columns;
		newItem.BoundingHeight = (ushort) rows;
		newItem.TexturePage = embed;
		Data.TexturePageItems.Add(newItem);
		
		UndertaleSprite.TextureEntry texentry = new UndertaleSprite.TextureEntry();
		texentry.Texture = newItem;
		newSprite.Textures.Add(texentry);
	}
	ChangeSelection(newSprite, true);
}

void cloneEmbeddedTexture()
{
	try
	{
		ImageConverter converter = new ImageConverter();
		Bitmap bitmap = new Bitmap(imgPath);
		bitmap.SetResolution(96.0F, 96.0F);

		originalWidth = bitmap.Width;
		originalHeight = bitmap.Height;
		Image img = new Bitmap(closestPower(originalWidth), closestPower(originalHeight));
		Graphics g = Graphics.FromImage(img);

		g.DrawImage(bitmap, 0, 0);
		bitmap = new Bitmap(img);

		embed = new UndertaleEmbeddedTexture();
		embed.Name = new UndertaleString("Texture " + Data.EmbeddedTextures.Count);
		embed.Scaled = 1;
		embed.TextureData.TextureBlob = (byte[]) converter.ConvertTo(bitmap, typeof(byte[]));
		Data.EmbeddedTextures.Add(embed);
	}
	catch(Exception ex)
	{
		//embed failure!! Laugh at this user
		ScriptMessage("Failed to import file \"" + imgPath + "\" due to: " + ex.Message);
	}
}

int closestPower(double val)
{
	return (int) Math.Pow(2, Math.Ceiling(Math.Log(val) / Math.Log(2)));
}