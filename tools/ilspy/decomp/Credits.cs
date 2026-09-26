using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Credits : BaseMenu
{
	public struct CreditsLine
	{
		public enum LineType
		{
			Heading,
			Single,
			Double,
			Icon,
			Spacer
		}

		public LineType Type;

		public string Role;

		public string Text;

		public string Url;

		public Resource<Texture2D> Icon;
	}

	private GameObject UnityContents;

	private List<CreditsLine> Lines = new List<CreditsLine>();

	private int AddedLineIndex;

	private float yOffset;

	private bool ManualScroll;

	private static float AutoScrollSpeed = 100f;

	private static float ManualScrollSpeed = 400f;

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		UnityContents = base.gameObject.FindChild("Viewport/Contents");
		PaperTextureAmount = 0f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		ManualScroll = false;
		yOffset = 0f - HudBehaviour.Instance.HudPanelRectTransform.rect.height;
		if (UnityContents.transform.childCount <= 0)
		{
			AddCreditsHeading("Simplified Chinese", "");
			AddCreditsLine("ALBERT!!", "");
			AddCreditsLine("LOVOB_CHUI", "");
			AddCreditsLine("单辰 (aka Shan Chen - pjc_0813@qq.com)", "");
			AddCreditsLine("姜天阳 (aka Cleave Jiang - Cleavejiang@hotmail.com)", "https://steamcommunity.com/profiles/76561198044724277/");
			AddCreditsLine("罗嘉诚", "");
			AddCreditsLine("Yoongmian勇面", "");
			AddCreditsSpacer();
			AddCreditsHeading("Traditional Chinese", "");
			AddCreditsLine("LOVOB_CHUI", "");
			AddCreditsLine("姜天陽 (aka Cleave Jiang - Cleavejiang@hotmail.com)", "https://steamcommunity.com/profiles/76561198044724277/");
			AddCreditsSpacer();
			AddCreditsHeading("Czech", "");
			AddCreditsLineWithIcon("Jiří Linhart (linhart.official@gmail.com)", "Small Dialog studio", "", GameCursor.SmallDialogStudio);
			AddCreditsSpacer();
			AddCreditsHeading("French", "");
			AddCreditsLine("Jean-Charles Rocher", "");
			AddCreditsLine("theosautiere", "");
			AddCreditsLine("Devil Max", "");
			AddCreditsSpacer();
			AddCreditsHeading("German", "");
			AddCreditsLine("Martin Herda (invisiblestrain@herda.net)", "");
			AddCreditsSpacer();
			AddCreditsHeading("Hungarian", "");
			AddCreditsLine("Attila 'Ateszkoma' Kurucz", "https://ateszkoma.blogspot.com");
			AddCreditsSpacer();
			AddCreditsHeading("Indonesian", "");
			AddCreditsLine("Darma Cahya Sani Adhi", "https://steamcommunity.com/profiles/76561198857884173/");
			AddCreditsLine("Felbie Suryafiandi Layardy", "");
			AddCreditsSpacer();
			AddCreditsHeading("Italian", "");
			AddCreditsLine("Michela (hagensruf@gmail.com)", "");
			AddCreditsLine("Filippo L", "");
			AddCreditsLine("Team Lagorn", "https://www.facebook.com/LagornIta/");
			AddCreditsSpacer();
			AddCreditsHeading("Japanese", "");
			AddCreditsLine("No Money No Honey", "https://steamcommunity.com/id/no_money_no_honey/");
			AddCreditsLine("Wonka", "https://steamcommunity.com/profiles/76561198201980015/");
			AddCreditsSpacer();
			AddCreditsHeading("Korean", "");
			AddCreditsLine("해무아빠(hemupapa)", "https://www.youtube.com/channel/UC71rwjGQam51i8vrb2XOFBg");
			AddCreditsSpacer();
			AddCreditsHeading("Lithuanian", "");
			AddCreditsLine("Audrius Vaidalauskis", "");
			AddCreditsSpacer();
			AddCreditsHeading("Polish", "");
			AddCreditsLine("Bartłomiej Wantoła", "Vantoro Polish Localizations", "https://www.patreon.com/cw/Vantoro");
			AddCreditsLine("Adam Drabiński", "");
			AddCreditsLine("Maggie Mojsiejuk", "https://maggiemakes.games/");
			AddCreditsLine("afore", "https://soundcloud.com/aforey");
			AddCreditsSpacer();
			AddCreditsHeading("Brazilian Portuguese", "");
			AddCreditsLine("Arthur 'Loki' Nunes", "");
			AddCreditsLine("Carlos 'Reaction' Vicenzi (carlos.vicenzi@hotmail.com)", "https://www.facebook.com/Reaction.Vicenzi/");
			AddCreditsLine("Kyo", "");
			AddCreditsLine("HaNDi", "");
			AddCreditsSpacer();
			AddCreditsHeading("Russian", "");
			AddCreditsLine("Pavel Borisov", "");
			AddCreditsSpacer();
			AddCreditsHeading("European Spanish", "");
			AddCreditsLine("Leonardo (aka System Tech)", "https://www.youtube.com/channel/UCIaEHgofmBgExHtcDVKOeMg");
			AddCreditsLine("Mario Cordoba Rodriguez", "");
			AddCreditsLine("Jesus Salazar", "");
			AddCreditsLine("Facundo López Bolado (flopezbt@hotmail.com)", "");
			AddCreditsLine("Samuel Gimenez (SheamusHD)", "");
			AddCreditsSpacer();
			AddCreditsHeading("Latin American Spanish", "");
			AddCreditsLine("Facundo López Bolado (flopezbt@hotmail.com)", "");
			AddCreditsLine("Ramiro Ra (ramiroecharte99@gmail.com)", "");
			AddCreditsLine("Truko Jara", "");
			AddCreditsSpacer();
			AddCreditsHeading("Turkish", "");
			AddCreditsLine("Yusuf Yılgör", "https://steamcommunity.com/profiles/76561198084160302");
			AddCreditsLine("Yaşar Tolunay Kınık", "https://steamcommunity.com/id/tlnyknk");
			AddCreditsLine("Ali Cem Atılgan", "");
			AddCreditsLine("Cihan Mert Özdemir", "");
			AddCreditsSpacer();
			AddCreditsHeading("Ukrainian", "");
			AddCreditsLine("Survager", "https://www.youtube.com/c/survager");
			AddCreditsSpacer();
			AddCreditsHeading("Vietnamese", "");
			AddCreditsLine("Trần Quốc Anh aka ShaoriSR (tranquocanhshaorisr@gmail.com)", "");
			AddCreditsSpacer();
			AddCreditsHeading("Music", "");
			AddCreditsLine("Alexandre Gatt", "");
			AddCreditsLine("Richard Camo", "https://www.youtube.com/channel/UCOElbYT3OmEsXsuooIbC2oA");
			AddCreditsSpacer();
			AddCreditsHeading("Sound Engineer", "");
			AddCreditsLine("Gevin Campos", "");
			AddCreditsSpacer();
			AddCreditsHeading("Zombie Sounds", "");
			AddCreditsLine("Alexander Matthews (aka PudgeWiggle - admatthews@mrbac.net)", "");
			AddCreditsSpacer();
			AddCreditsHeading("Steam Community Items", "");
			AddCreditsLine("Ramiro Ra (ramiroecharte99@gmail.com)", "");
			AddCreditsSpacer();
			AddCreditsHeading("Icon Archive", "http://www.iconarchive.com");
			AddCreditsLine("Wedding Rings Icon", "Aha-Soft", "http://www.iconarchive.com/show/free-large-love-icons-by-aha-soft/Wedding-Rings-icon.html");
			AddCreditsLine("Syringe Icon", "DevCom", "http://www.iconarchive.com/show/medical-icons-by-devcom/syringe-icon.html");
			AddCreditsLine("Industry Biohazard Icon", "Icons8", "http://www.iconarchive.com/show/windows-8-icons-by-icons8/Industry-Biohazard-icon.html");
			AddCreditsLine("Editing Cut Icon", "Icons8", "http://www.iconarchive.com/show/ios7-icons-by-icons8/Editing-Cut-icon.html");
			AddCreditsLine("Rain Drop Icon", "IconsMind", "http://www.iconarchive.com/show/outline-icons-by-iconsmind/Rain-Drop-icon.html");
			AddCreditsLine("Tree Icon", "PelFusion", "http://www.iconarchive.com/show/christmas-shadow-icons-by-pelfusion/Tree-icon.html");
			AddCreditsLine("Travel Campfire Icon", "Icons8", "http://www.iconarchive.com/show/ios7-icons-by-icons8/Travel-Campfire-icon.html");
			AddCreditsLine("Umbrella 2 Icon", "IconsMind", "http://www.iconarchive.com/show/outline-icons-by-iconsmind/Umbrella-2-icon.html");
			AddCreditsLine("Food Cooking Pot Icon", "Icons8", "http://www.iconarchive.com/show/ios7-icons-by-icons8/Food-Cooking-Pot-icon.html");
			AddCreditsLine("Messaging Sad Icon", "Icons8", "http://www.iconarchive.com/show/ios7-icons-by-icons8/Messaging-Sad-icon.html");
			AddCreditsLine("Network Shield Icon", "Icons8", "http://www.iconarchive.com/show/ios7-icons-by-icons8/Network-Shield-icon.html");
			AddCreditsLine("Dog Paw Icon", "Glyphish", "http://www.iconarchive.com/show/glyphish-icons-by-glyphish/82-dog-paw-icon.html");
			AddCreditsLine("Finance Money Bag Icon", "Icons8", "http://www.iconarchive.com/show/ios7-icons-by-icons8/Finance-Money-Bag-icon.html");
			AddCreditsLine("Apps preferences desktop mouse Icon", "Oxygen Team", "http://www.iconarchive.com/show/oxygen-icons-by-oxygen-icons.org/Apps-preferences-desktop-mouse-icon.html");
			AddCreditsLine("Household Shovel Icon", "Icons8", "http://www.iconarchive.com/show/ios7-icons-by-icons8/Household-Shovel-icon.html");
			AddCreditsLine("Horror knife 2 Icon", "Icons8", "http://www.iconarchive.com/show/halloween-icons-by-icons8/horror-knife-2-icon.html");
			AddCreditsLine("Door Icon", "IconsMind", "http://www.iconarchive.com/show/outline-icons-by-iconsmind/Door-icon.html");
			AddCreditsLine("Very Basic Speech Bubble Icon", "Icons8", "http://www.iconarchive.com/show/ios7-icons-by-icons8/Very-Basic-Speech-Bubble-icon.html");
			AddCreditsLine("Folder Open 2 Icon", "IconsMind", "http://www.iconarchive.com/show/outline-icons-by-iconsmind/Folder-Open-2-icon.html");
			AddCreditsLine("Very Basic Link Icon", "Icons8", "http://www.iconarchive.com/show/windows-8-icons-by-icons8/Very-Basic-Link-icon.html");
			AddCreditsLine("Sports Drop Zone Icon", "Icons8", "https://iconarchive.com/show/windows-8-icons-by-icons8/Sports-Drop-Zone-icon.html");
			AddCreditsLine("User Interface Error Icon", "Icons8", "https://iconarchive.com/show/ios7-icons-by-icons8/User-Interface-Error-icon.html");
			AddCreditsLine("Animals Deer Icon", "Icons8", "https://iconarchive.com/show/windows-8-icons-by-icons8/Animals-Deer-icon.html");
			AddCreditsLine("Ecommerce Gift Icon", "Icons8", "https://iconarchive.com/show/ios7-icons-by-icons8/Ecommerce-Gift-icon.html");
			AddCreditsLine("Diy Hammer Icon", "Icons8", "https://www.iconarchive.com/show/windows-8-icons-by-icons8/Diy-Hammer-icon.html");
			AddCreditsLine("Flower Icon", "Pictogrammers Team", "https://www.iconarchive.com/show/material-icons-by-pictogrammers/flower-icon.html");
			AddCreditsLine("Flower poppy Icon", "Pictogrammers Team", "https://www.iconarchive.com/show/material-icons-by-pictogrammers/flower-poppy-icon.html");
			AddCreditsLine("Earth Icon", "ionic", "https://www.iconarchive.com/show/ionicons-icons-by-ionic/earth-icon.html");
			AddCreditsLine("Cursor pointer Icon", "Iconoir Team", "https://www.iconarchive.com/show/iconoir-icons-by-iconoir-team/cursor-pointer-icon.html");
			AddCreditsLine("Hand right outline Icon", "ionic", "https://www.iconarchive.com/show/ionicons-icons-by-ionic/hand-right-outline-icon.html");
			AddCreditsLine("Finance Cash Receiving Icon", "Icons8", "https://www.iconarchive.com/show/ios7-icons-by-icons8/Finance-Cash-Receiving-icon.html");
			AddCreditsSpacer();
			AddCreditsHeading("FlatIcon", "https://www.flaticon.com/");
			AddCreditsLine("Salad", "Those Icons", "https://www.flaticon.com/free-icon/salad_639065");
			AddCreditsLine("Nose", "Freepik", "https://www.flaticon.com/free-icon/nose_105385");
			AddCreditsLine("Ear", "Freepik", "https://www.flaticon.com/free-icon/ear_105374");
			AddCreditsLine("Human Eye", "Freepik", "https://www.flaticon.com/free-icon/human-eye_105371");
			AddCreditsLine("Cooking", "Freepik", "https://www.flaticon.com/free-icon/cooking_113339");
			AddCreditsLine("Navigation", "Kiranshastry", "https://www.flaticon.com/free-icon/navigation_1217387");
			AddCreditsLine("Swords", "Freepik", "https://www.flaticon.com/free-icon/swords_158107");
			AddCreditsLine("Snow", "Freepik", "https://www.flaticon.com/free-icon/snow_1207679");
			AddCreditsLine("Pickaxe", "Smashicons", "https://www.flaticon.com/free-icon/pickaxe_663519");
			AddCreditsLine("Networking", "Gregor Cresnar", "https://www.flaticon.com/free-icon/networking_179702");
			AddCreditsLine("Sum", "prettycons", "https://www.flaticon.com/free-icon/sum_2000491");
			AddCreditsLine("Strength", "Becris", "https://www.flaticon.com/free-icon/strength_349383");
			AddCreditsLine("Machete", "Freepik", "https://www.flaticon.com/free-icon/machete_1703325");
			AddCreditsLine("Gun", "Freepik", "https://www.flaticon.com/free-icon/gun_1022348");
			AddCreditsLine("Archery", "Freepik", "https://www.flaticon.com/free-icon/archery_2062196");
			AddCreditsLine("Ninja", "Freepik", "https://www.flaticon.com/free-icon/ninja_144587");
			AddCreditsLine("Trap", "Freepik", "https://www.flaticon.com/free-icon/trap_1167721");
			AddCreditsLine("Rifle", "Freepik", "https://www.flaticon.com/free-icon/rifle_238963");
			AddCreditsLine("Gun", "Smashicons", "https://www.flaticon.com/free-icon/gun_4611485");
			AddCreditsLine("Gun", "Freepik", "https://www.flaticon.com/free-icon/gun_1022348");
			AddCreditsLine("Devil", "Eucalyp", "https://www.flaticon.com/free-icon/devil_2941657");
			AddCreditsLine("Combat", "Eucalyp", "https://www.flaticon.com/free-icon/combat_1119899");
			AddCreditsLine("Road With Broken Line", "Freepik", "https://www.flaticon.com/free-icon/road-with-broken-line_62512");
			AddCreditsLine("Houses", "Freepik", "https://www.flaticon.com/free-icon/houses_1191557");
			AddCreditsLine("Pads", "Smashicons", "https://www.flaticon.com/free-icon/pads_1412874");
			AddCreditsLine("Mask", "Freepik", "https://www.flaticon.com/free-icon/mask_883043");
			AddCreditsLine("Crown", "Stockio", "https://www.flaticon.com/free-icon/crown_658098");
			AddCreditsLine("Circular Arrow", "Freepik", "https://www.flaticon.com/free-icons/undo");
			AddCreditsLine("Check", "Pixel perfect", "https://www.flaticon.com/free-icons/tick");
			AddCreditsLine("Rifle", "Freepik", "https://www.flaticon.com/free-icon/rifle_486230");
			AddCreditsLine("Pistol", "Georgy", "https://www.flaticon.com/free-icon/pistol_4776179");
			AddCreditsLine("Heart", "Freepik", "https://www.flaticon.com/free-icon/heart_10562487");
			AddCreditsLine("Discord free icon", "Hight Quality Icons", "https://www.flaticon.com/free-icon/discord_3670157");
			AddCreditsLine("Relieved", "Rahul Kaklotar", "https://www.flaticon.com/free-icon/relieved_8376648");
			AddCreditsSpacer();
			AddCreditsHeading("Public Domain Pictures", "https://www.publicdomainpictures.net");
			AddCreditsLine("Crucifix", "George Hodan", "https://www.publicdomainpictures.net/en/view-image.php?image=170148&picture=crucifix");
			AddCreditsLine("Ufo", "kai Stachowiak", "https://www.publicdomainpictures.net/en/view-image.php?image=203737&picture=ufo");
			AddCreditsLine("Keep Out", "Ken E", "https://www.publicdomainpictures.net/en/view-image.php?image=176310&picture=keep-out");
			AddCreditsLine("Planet Fantasy", "RAJESH misra", "https://www.publicdomainpictures.net/en/view-image.php?image=151841&picture=planet-fantasy");
			AddCreditsLine("Planet Fantasy 4", "RAJESH misra", "https://www.publicdomainpictures.net/en/view-image.php?image=165092&picture=planet-fantasy-4");
			AddCreditsLine("Planet Fantasy 6", "RAJESH misra", "https://www.publicdomainpictures.net/en/view-image.php?image=165094&picture=planet-fantasy-6");
			AddCreditsLine("Marble Background", "MALIZ ONG", "https://www.publicdomainpictures.net/en/view-image.php?image=39332&picture=marble-background");
			AddCreditsLine("Blue Marble Background", "Alex Borland", "https://www.publicdomainpictures.net/en/view-image.php?image=209585&picture=blue-marble-background");
			AddCreditsLine("Nails", "Charles Rondeau", "https://www.publicdomainpictures.net/en/view-image.php?image=96003&picture=nails");
			AddCreditsLine("Bald Eagle And A Flag", "Petr Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=47412&picture=bald-eagle-and-a-flag");
			AddCreditsLine("Delicious Goulash Cooking", "Alex Borland", "https://www.publicdomainpictures.net/en/view-image.php?image=220340&picture=delicious-goulash-cooking");
			AddCreditsLine("Sword", "晓霞 赵", "https://www.publicdomainpictures.net/en/view-image.php?image=201148&picture=sword");
			AddCreditsLine("Buck With Antlers Forest", "Ken Kistler", "https://www.publicdomainpictures.net/en/view-image.php?image=100242&picture=buck-with-antlers-forest");
			AddCreditsLine("Young Nurse", "Petr Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=247235&picture=young-nurse");
			AddCreditsLine("Power Drill Tool", "Petr Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=18135&picture=power-drill-tool");
			AddCreditsLine("Background Wallpaper", "kai Stachowiak", "https://www.publicdomainpictures.net/en/view-image.php?image=205064&picture=background-wallpaper");
			AddCreditsLine("Man Working Out In Gym", "mohamed mohamed mahmoud hassan", "https://www.publicdomainpictures.net/en/view-image.php?image=228228&picture=man-working-out-in-gym");
			AddCreditsLine("Corn Fields Of Georgia, USA", "Paul Brennan", "https://www.publicdomainpictures.net/en/view-image.php?image=90262&picture=corn-fields-of-georgia-usa");
			AddCreditsLine("Ball Of Hemp Twine", "George Hodan", "https://www.publicdomainpictures.net/en/view-image.php?image=60538&picture=ball-of-hemp-twine");
			AddCreditsLine("Notebook With Pen", "Piotr Wojtkowski", "https://www.publicdomainpictures.net/en/view-image.php?image=15769&picture=notebook-with-pen");
			AddCreditsLine("Notebook Paper On Cement Wall", "icon0 com", "https://www.publicdomainpictures.net/en/view-image.php?image=341809&picture=notebook-paper-on-cement-wall");
			AddCreditsLine("Two Slices Of Bacon", "Charles Rondeau", "https://www.publicdomainpictures.net/en/view-image.php?image=109419&picture=two-slices-of-bacon");
			AddCreditsLine("Delicious Meal", "Kevin Phillips", "https://www.publicdomainpictures.net/en/view-image.php?image=145766&picture=delicious-meal");
			AddCreditsLine("Grilled Chicken Wings", "Olga Konstantinova", "https://www.publicdomainpictures.net/en/view-image.php?image=409750&picture=grilled-chicken-wings");
			AddCreditsLine("Egg", "Vera Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=8793&picture=egg");
			AddCreditsLine("Corn", "Jean Beaufort", "https://www.publicdomainpictures.net/en/view-image.php?image=228308&picture=corn");
			AddCreditsLine("Pumpkin Seeds", "Peter Griffin", "https://www.publicdomainpictures.net/en/view-image.php?image=69022&picture=pumpkin-seeds");
			AddCreditsLine("Boiled Egg", "Marina Shemesh", "https://www.publicdomainpictures.net/en/view-image.php?image=17615&picture=boiled-egg");
			AddCreditsLine("Cappuccino", "Petr Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=318515&picture=cappuccino");
			AddCreditsLine("Coffee With Whipped Cream", "Ian L", "https://www.publicdomainpictures.net/en/view-image.php?image=171752&picture=coffee-with-whipped-cream");
			AddCreditsLine("Snack Potato Chip", "icon0", "https://www.publicdomainpictures.net/en/view-image.php?image=344481&picture=snack-potato-chip");
			AddCreditsLine("Police Shotgun", "Alex Borland", "https://www.publicdomainpictures.net/en/view-image.php?image=482557&picture=remington-870-police-magnum-shotgun");
			AddCreditsLine("Grunge American Flag", "Dawn Hudson", "https://www.publicdomainpictures.net/en/view-image.php?image=114717&picture=grunge-american-flag");
			AddCreditsLine("Machine Gun With Bullets", "Alex Borland", "https://www.publicdomainpictures.net/en/view-image.php?image=134070&picture=machine-gun-with-bullets");
			AddCreditsLine("American Flag Eagle", "Linnaea Mallette", "https://www.publicdomainpictures.net/en/view-image.php?image=500134&picture=american-flag-eagle");
			AddCreditsLine("Labor Day Worker", "Petr Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=186753&picture=labor-day-worker");
			AddCreditsLine("Woman In A Hard Hat", "Petr Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=381268&picture=woman-in-a-hard-hat");
			AddCreditsLine("Tool Kit", "Anna Langova", "https://www.publicdomainpictures.net/en/view-image.php?image=2441&picture=tool-kit");
			AddCreditsLine("American Flag", "Piotr Siedlecki", "https://www.publicdomainpictures.net/en/view-image.php?image=156822&picture=american-flag");
			AddCreditsLine("Hibiscus Flower", "Petr Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=4305&picture=hibiscus-flower");
			AddCreditsLine("Blossom Flower Garden Nature", "Andrea Stöckel", "https://www.publicdomainpictures.net/en/view-image.php?image=308642&picture=blossom-flower-garden-nature");
			AddCreditsLine("Man, Back, Model, Posing, Muscles", "Victoria Borodinova", "https://www.publicdomainpictures.net/en/view-image.php?image=477782&picture=man-back-model-posing-muscles");
			AddCreditsLine("Costume Jewelry", "Junior Libby", "https://www.publicdomainpictures.net/en/view-image.php?image=24938&picture=costume-jewelry");
			AddCreditsLine("Sunny-side-up (fried Eggs)", "yamada taro", "https://www.publicdomainpictures.net/en/view-image.php?image=15362&picture=sunny-side-up-fried-eggs");
			AddCreditsLine("Old Key", "George Hodan", "https://www.publicdomainpictures.net/en/view-image.php?image=62976&picture=old-key");
			AddCreditsLine("Repair vector icon", "Openclipart", "https://publicdomainvectors.org/en/free-clipart/Repair-vector-icon/9226.html");
			AddCreditsLine("Children Archery Set", "Jakub Onderka", "https://www.publicdomainpictures.net/en/view-image.php?image=427175&picture=children-archery-set");
			AddCreditsLine("Woman With A Drill", "Petr Kratochvil", "https://www.publicdomainpictures.net/en/view-image.php?image=392977&picture=woman-with-a-drill");
			AddCreditsLine("Sunflower Field", "Ian L", "https://www.publicdomainpictures.net/en/view-image.php?image=141044&picture=sunflower-field");
			AddCreditsLine("Musketeer, Fencing, Epee, Fantasy", "Victoria Borodinova", "https://www.publicdomainpictures.net/en/view-image.php?image=426176&picture=musketeer-fencing-epee-fantasy");
			AddCreditsLine("Old Paper Cardboard Parchment", "Martina Stokow", "https://www.publicdomainpictures.net/en/view-image.php?image=461076&picture=old-paper-cardboard-parchment");
			AddCreditsLine("Yoga Woman Sunset Beach", "Karen Arnold", "https://www.publicdomainpictures.net/en/view-image.php?image=255159&picture=yoga-woman-sunset-beach");
			AddCreditsLine("Pills On Blue Background", "Circe Denyer", "https://www.publicdomainpictures.net/en/view-image.php?image=320816&picture=pills-on-blue-background");
			AddCreditsLine("Amoxicillin Pills Green Background", "Circe Denyer", "https://www.publicdomainpictures.net/en/view-image.php?image=320818&picture=amoxicillin-pills-green-background");
			AddCreditsSpacer();
			AddCreditsHeading("FreeDigitalPhotos.net", "http://www.freedigitalphotos.net");
			AddCreditsLine("Fried Chicken Leg", "piyato", "");
			AddCreditsLine("Fried Chicken", "audfriday13", "");
			AddCreditsLine("Fast Food", "rakratchada torsap", "");
			AddCreditsLine("Cheeseburger On Wooden Board", "Grant Cochrane", "");
			AddCreditsLine("Vegetable Oil In A Plastic Bottle And Jar On White Background", "SOMMAI", "https://www.freedigitalphotos.net/images/vegetable-oil-in-a-plastic-bottle-and-jar-on-white-background-photo-p194252");
			AddCreditsSpacer();
			AddCreditsHeading("freepik", "https://www.freepik.com/");
			AddCreditsLine("Omelette with salad of cucumber, tomato,corn and herbs in rustic style", "Kamran Aydinov", "https://www.freepik.com/free-photo/omelette-with-salad-cucumber-tomato-corn-herbs-rustic-style_5938329.htm");
			AddCreditsLine("Close-up view of corn cob with fork on black wall", "stockking", "https://www.freepik.com/free-photo/close-up-view-corn-cob-with-fork-black-wall_9206880.htm");
			AddCreditsLine("car engine", "fxquadro", "https://www.freepik.com/free-photo/female-model-with-tattooed-body-wearing-protective-goggles-car-engine_28992589.htm");
			AddCreditsLine("Pulley isolated on transparent background", "tohamina", "https://www.freepik.com/free-psd/pulley-isolated-transparent-background_134485373.htm");
			AddCreditsSpacer();
			AddCreditsHeading("OpenClipArt", "https://openclipart.org");
			AddCreditsLine("Misc Bag Toolbox Red", "glitch", "https://openclipart.org/detail/210207/misc-bag-toolbox-red");
			AddCreditsLine("Refugees Welcome (not so heteronormative)", "alice-d25", "https://openclipart.org/detail/213192/refugees-welcome-not-so-heteronormative");
			AddCreditsLine("Skull and Crossbones", "ryanlerch", "https://openclipart.org/detail/1448/skull-and-crossbones");
			AddCreditsLine("Trash Can", "amites", "https://openclipart.org/detail/226230/trash-can");
			AddCreditsSpacer();
			AddCreditsHeading("TurboSquid", "https://www.turbosquid.com");
			AddCreditsLine("Old Man", "paultosca", "https://www.turbosquid.com/3d-models/free-x-model-old-man/864833");
			AddCreditsLine("WatchTower", "Gerzi 3d Art", "https://www.turbosquid.com/3d-models/free-watchtower-games-unreal-3d-model/446421");
			AddCreditsLine("High Definition Billboard", "Ecleposs", "https://www.turbosquid.com/3d-models/3d-definition-billboard-model/344607");
			AddCreditsLine("Outhouse", "tbs4life", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/363557");
			AddCreditsLine("Mini Mart", "3D_Solutions", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/547508");
			AddCreditsLine("Body Rabbit model", "3D_wanderer", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/1286324");
			AddCreditsLine("3D Fried Rabbit", "3d_wanderer", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/1287082");
			AddCreditsLine("Realistic Wooden Chest", "bazsem", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/1090120");
			AddCreditsLine("Old Rusty Car 3D model", "Changyoung Sung", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/1320167");
			AddCreditsLine("Rusty Car Collection", "createddd", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/1092131");
			AddCreditsLine("Charcoal Kiln", "Kamiomi", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/333416");
			AddCreditsLine("Still", "uncle808us", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/409084");
			AddCreditsLine("Plastic Bucket", "butteryoatmorsels", "https://www.turbosquid.com/3d-models/free-plastic-bucket-3d-model/978220");
			AddCreditsLine("FM Radio Game Ready Pbr", "BatuhanOZER", "https://www.turbosquid.com/FullPreview/Index.cfm/ID/1112850");
			AddCreditsLine("Felt Hat", "ebuz", "https://www.turbosquid.com/3d-models/free-hat-3d-model/710945");
			AddCreditsLine("Radio", "AndrijaAlp", "https://www.turbosquid.com/3d-models/free-3ds-model-asset/1127601");
			AddCreditsLine("3D Chicken", "coolnidz", "https://www.turbosquid.com/3d-models/3d-chicken-1759190");
			AddCreditsLine("3D Next Gen Cote Hovel Chicken Coop Cage", "Enterables", "https://www.turbosquid.com/3d-models/3d-gen-hovel-chicken-1502926");
			AddCreditsLine("Water Trough", "Kamiomi", "https://www.turbosquid.com/3d-models/free-historical-water-trough-3d-model/333094");
			AddCreditsLine("Gibbet Cage with skeleton 3D model", "Simon_Green", "https://www.turbosquid.com/3d-models/gibbet-cage-with-skeleton-3d-model-1919672");
			AddCreditsLine("Soda Can", "pozzypro", "https://www.turbosquid.com/3d-models/free-c4d-mode-soda/868574");
			AddCreditsLine("3D model Bow quiver and arrows", "Surin Egor", "https://www.turbosquid.com/3d-models/3d-model-bow-quiver-arrows-1285491");
			AddCreditsLine("Silex", "Documedia", "https://www.turbosquid.com/3d-models/3ds-silex-knife/778444");
			AddCreditsLine("Mortar and Pestle Old model", "neyova", "https://www.turbosquid.com/3d-models/mortar-and-pestle-old-model-1736286");
			AddCreditsLine("3D model Motel Sign - Game Ready", "meYoouunng", "https://www.turbosquid.com/3d-models/3d-model-realistic-motel-sign-ready-1270186");
			AddCreditsLine("3d_tent1", "jo2bigornia", "https://www.turbosquid.com/3d-models/tent-camping-outing-max-free/579765");
			AddCreditsSpacer();
			AddCreditsHeading("Nasa", "https://www.nasa.gov/");
			AddCreditsLine("Star Map", "", "https://svs.gsfc.nasa.gov/cgi-bin/details.cgi?aid=3572");
			AddCreditsSpacer();
			AddCreditsHeading("FontSquirrel", "https://www.fontsquirrel.com");
			AddCreditsLine("Komika Text", "Apostrophic Labs & Pavel Borisov", "https://www.fontsquirrel.com/fonts/Komika-Text");
			AddCreditsLine("Komika Axis", "Apostrophic Labs & Pavel Borisov", "https://www.fontsquirrel.com/fonts/Komika-Axis");
			AddCreditsLine("True Crimes", "Walter Velez", "https://www.fontsquirrel.com/fonts/True-Crimes");
			AddCreditsLine("Poetsen", "Impallari Type", "https://www.fontsquirrel.com/fonts/poetsen");
			AddCreditsLine("Luckiest Guy", "Astigmatic", "https://www.fontsquirrel.com/fonts/luckiest-guy");
			AddCreditsLine("Helsinki", "Vic Fieger", "https://www.fontsquirrel.com/fonts/helsinki");
			AddCreditsLine("Comic Relief", "Loudifier", "https://www.fontsquirrel.com/fonts/comic-relief");
			AddCreditsLine("Mitr", "Cadson Demak", "https://www.fontsquirrel.com/fonts/mitr");
			AddCreditsSpacer();
			AddCreditsHeading("Google Fonts", "https://fonts.google.com/");
			AddCreditsLine("Noto Sans", "Google", "https://fonts.google.com/specimen/Noto+Sans");
			AddCreditsSpacer();
			AddCreditsHeading("Online Fonts", "https://online-fonts.com/");
			AddCreditsLine("Komika Title - Axis RUS-LAT", "Apostrophic Labs & WolfBainX", "https://online-fonts.com/fonts/komika-title-axis-rus-lat");
			AddCreditsSpacer();
			AddCreditsHeading("FreeSound", "http://www.freesound.org/");
			AddCreditsLine("Waterfall, Small » Waterfall, Small, C.wav", "InspectorJ", "https://freesound.org/people/InspectorJ/sounds/365920/");
			AddCreditsLine("Rivers » Small River 1 - Slow - Semi close", "Pfannkuchn", "https://freesound.org/people/Pfannkuchn/sounds/459412/");
			AddCreditsLine("Weather » Ambiance - Heavy Rain Loop", "D W", "http://www.freesound.org/people/D%20W/sounds/136971/");
			AddCreditsLine("Inside an abandoned lead mine » Rockfall in mine.wav", "Benboncan", "http://freesound.org/people/Benboncan/sounds/60085/");
			AddCreditsLine("Punch_02", "thefsoundman", "http://freesound.org/people/thefsoundman/sounds/118513/");
			AddCreditsLine("Bloody Blade", "Kreastricon62", "https://freesound.org/people/Kreastricon62/sounds/323525/");
			AddCreditsLine("Bloody Blade 2", "Kreastricon62", "https://freesound.org/people/Kreastricon62/sounds/323526/");
			AddCreditsLine("Gore Splat", "ThefitzyG", "https://freesound.org/people/ThefitzyG/sounds/414296/");
			AddCreditsLine("Felling a tree with an axe", "tomattka", "https://freesound.org/people/tomattka/sounds/401730/");
			AddCreditsLine("2017 august - crickets 02", "Anthousai", "https://freesound.org/people/Anthousai/sounds/405644/");
			AddCreditsLine("AMB_S_Ext_Eastern_Oregon_Wildlife6", "conleec", "https://freesound.org/people/conleec/sounds/176786/");
			AddCreditsLine("A Tree Falling Down", "ecfike", "https://freesound.org/people/ecfike/sounds/139952/");
			AddCreditsLine("MatchBox - Strike and Light 03", "JarredGibb", "https://freesound.org/people/JarredGibb/sounds/248241/");
			AddCreditsLine("Fire » Firesteel", "Benboncan", "https://freesound.org/people/Benboncan/sounds/66457/");
			AddCreditsLine("Fire sounds » Bonfire", "juskiddink", "https://freesound.org/people/juskiddink/sounds/65795/");
			AddCreditsLine("stab sweetner", "jeseid77", "https://freesound.org/people/jeseid77/sounds/83681/");
			AddCreditsLine("Archery » ArrowHit03", "Yap_Audio_Production", "https://freesound.org/people/Yap_Audio_Production/sounds/218462/");
			AddCreditsLine("Realistic Arrow » Regular Arrow Shot", "brendan89", "https://freesound.org/people/brendan89/sounds/321552/");
			AddCreditsLine("Bow & Arrows » Bow Pull", "LiamG_SFX", "https://freesound.org/people/LiamG_SFX/sounds/322215/");
			AddCreditsLine("arrow_clatter", "smcameron", "https://freesound.org/people/smcameron/sounds/50772/");
			AddCreditsLine("shovel digging sound", "JJDG", "https://freesound.org/people/JJDG/sounds/441824/");
			AddCreditsLine("Pickaxe Striking Rock", "Benboncan", "https://freesound.org/people/Benboncan/sounds/71823/");
			AddCreditsLine("Peeing outside", "Adam_N", "https://freesound.org/people/Adam_N/sounds/346674/");
			AddCreditsLine("car burning.wav", "Hssmusic", "");
			AddCreditsLine("hammering12.wav", "WIM", "");
			AddCreditsLine("Breathing.wav", "scarbelly25", "");
			AddCreditsLine("thud bassy slam.aiff", "kyles", "");
			AddCreditsLine("another whoosh pair.flac", "Timbre", "");
			AddCreditsLine("Hallelujah", "magixmusic", "");
			AddCreditsLine("Destruction, Wooden, A", "InspectorJ", "https://freesound.org/people/InspectorJ/sounds/352513/");
			AddCreditsLine("bush6", "schademans", "https://freesound.org/people/schademans/sounds/2595/");
			AddCreditsLine("Blood Hitting Window", "Rock Savage", "https://freesound.org/people/Rock%20Savage/sounds/81042/");
			AddCreditsLine("throw", "marchon11", "https://freesound.org/people/marchon11/sounds/493224/");
			AddCreditsLine("Acoustic Guitar » Ambient Acoustic", "StrangerEight", "https://freesound.org/people/StrangerEight/sounds/148695/");
			AddCreditsLine("Ipa Beatbox Kit 01 » splat.wav", "ipaghost", "https://freesound.org/people/ipaghost/sounds/335794/");
			AddCreditsLine("fruitbite.ogg", "metekavruk", "https://freesound.org/people/metekavruk/sounds/348271/");
			AddCreditsLine("splat 005.wav", "yottasounds", "https://freesound.org/people/yottasounds/sounds/232135/");
			AddCreditsLine("drone11.wav", "LG", "https://freesound.org/people/LG/sounds/24071/");
			AddCreditsLine("robinalarm", "wildear1", "https://freesound.org/people/wildear1/sounds/94997/");
			AddCreditsLine("Spotted Owl2 no noise", "lttldude9", "https://freesound.org/people/lttldude9/sounds/259659/");
			AddCreditsLine("long eared owl", "AndrewJonesFoto", "https://freesound.org/people/AndrewJonesFoto/sounds/434574/");
			AddCreditsLine("owl call nearby mount royal", "kyles", "https://freesound.org/people/kyles/sounds/452095/");
			AddCreditsLine("R01-28-Pigeon Coos", "craigsmith", "https://freesound.org/people/craigsmith/sounds/479589/");
			AddCreditsLine("Nighthawk swoosh 1&2", "Danjocross", "https://freesound.org/people/Danjocross/sounds/164201/");
			AddCreditsLine("Vaux's Swift", "daveincamas", "https://freesound.org/people/daveincamas/sounds/122174/");
			AddCreditsLine("red headed woodpecker", "JPBILLINGSLEYJR", "https://freesound.org/people/JPBILLINGSLEYJR/sounds/464886/");
			AddCreditsLine("blue jay", "cognito perceptu", "https://freesound.org/people/cognito%20perceptu/sounds/57906/");
			AddCreditsLine("YellowWarbler1", "Coppersmith Barbet", "https://freesound.org/people/Coppersmith%20Barbet/sounds/33378/");
			AddCreditsLine("red_tail_hawk 1&2", "wisslgisse", "https://freesound.org/people/wisslgisse/sounds/52756/");
			AddCreditsLine("Milagra", "foosiemac", "https://freesound.org/people/foosiemac/sounds/76797/");
			AddCreditsLine("Car_Door_Close_02", "Meisben", "https://freesound.org/people/Meisben/sounds/488052/");
			AddCreditsLine("Car sounds » 10 Door opening", "15HPanska_Ruttner_Jan", "https://freesound.org/people/15HPanska_Ruttner_Jan/sounds/461684/");
			AddCreditsLine("Videogame Menu BUTTON CLICK", "Christopherderp", "https://freesound.org/people/Christopherderp/sounds/342200/");
			AddCreditsLine("Videogame Menu Button Clicking Sound 17", "Christopherderp", "https://freesound.org/people/Christopherderp/sounds/333042/");
			AddCreditsLine("Button Simple 01", "Jaoreir", "https://freesound.org/people/Jaoreir/sounds/533567/");
			AddCreditsLine("transitions » pop.wav", "anagar", "https://freesound.org/people/anagar/sounds/267952/");
			AddCreditsLine("button", "Leszek_Szary", "https://freesound.org/people/Leszek_Szary/sounds/146718/");
			AddCreditsLine("Clean Revolver Reload", "Dredile", "https://freesound.org/people/Dredile/sounds/177863/");
			AddCreditsLine("Weapons » 357 Magnum Ext 2 shots Empty Chamber Clicks", "klangfabrik", "https://freesound.org/people/klangfabrik/sounds/232867/");
			AddCreditsLine("snowball » sfx_snowball_hit-01", "bajko", "https://freesound.org/people/bajko/sounds/378057/");
			AddCreditsLine("snowball » sfx_snowball_hit-02", "bajko", "https://freesound.org/people/bajko/sounds/378059/");
			AddCreditsLine("snowball » sfx_snowball_hit-03", "bajko", "https://freesound.org/people/bajko/sounds/378058/");
			AddCreditsLine("Chicks Peep", "GB01", "https://freesound.org/people/GB01/sounds/150298/");
			AddCreditsLine("Knocking on the door", "oldhiccup", "https://freesound.org/people/oldhiccup/sounds/567607/");
			AddCreditsLine("cartoon peek behind bush.wav", "elektroproleter", "https://freesound.org/people/elektroproleter/sounds/157567/");
			AddCreditsLine("windinleaves.wav", "klangfabrik", "https://freesound.org/people/klangfabrik/sounds/159606/");
			AddCreditsLine("reed-grass in wind 01.wav", "klankbeeld", "https://freesound.org/people/klankbeeld/sounds/323492/");
			AddCreditsLine("Snore.wav", "Juan_Merie_Venter", "https://freesound.org/people/Juan_Merie_Venter/sounds/327680/");
			AddCreditsLine("yawn-2.wav", "fer_t", "https://freesound.org/people/fer_t/sounds/94295/");
			AddCreditsLine("VocalPB.wav", "pjboyd", "https://freesound.org/people/pjboyd/sounds/180622/");
			AddCreditsLine("Warsongs » HELICOPTER", "isaac_arva", "https://freesound.org/people/isaac_arva/sounds/612735/");
			AddCreditsLine("screeching tyres / tires", "fractanimal", "https://freesound.org/people/fractanimal/sounds/614627/");
			AddCreditsLine("Feli Ignite's Sound Specials Series Impacts » Car Crash Elements Mix 01", "FeliUsers", "https://freesound.org/people/FeliUsers/sounds/682370/");
			AddCreditsLine("car_engine_won't start.wav", "KRAFTWERK2K1", "https://freesound.org/people/KRAFTWERK2K1/sounds/32416/");
			AddCreditsSpacer();
			AddCreditsHeading("FreeSFX", "http://www.freesfx.co.uk");
			AddCreditsLine("Dog Powerful Attack", "summercaesarguy", "");
			AddCreditsLine("Funny Bite", "zarabadeu", "");
			AddCreditsLine("heart beat", "mosha2010", "");
			AddCreditsLine("Comedy Bubble Pop", "mckinneysound", "");
			AddCreditsLine("Gun Cock", "mckinneysound", "");
			AddCreditsLine("Gun Dry Fire", "mckinneysound", "");
			AddCreditsLine("Multimedia Sounds", "mckinneysound", "");
			AddCreditsLine("Wooden Door", "mckinneysound", "");
			AddCreditsLine("Desk Bell", "stuartduffield", "");
			AddCreditsLine("Menu Wrong 1", "stuartduffield", "");
			AddCreditsLine("Old Horror Gate", "aelfric3333", "");
			AddCreditsLine("Vodka In GlassBottle Movement", "flowfx", "");
			AddCreditsSpacer();
			AddCreditsHeading("Unity Asset Store", "https://assetstore.unity.com");
			AddCreditsLine("PBR Barrel", "Lapis Edge", "https://assetstore.unity.com/packages/3d/props/pbr-barrel-50821");
			AddCreditsLine("PBR Pickaxe", "Shadowball Games", "https://assetstore.unity.com/packages/3d/props/tools/pbr-pickaxe-33718");
			AddCreditsLine("Medieval Forge", "Ricochet", "https://assetstore.unity.com/packages/3d/props/medieval-forge-119462");
			AddCreditsLine("Outdoor Pack Vol.1", "DigitalKonstrukt", "https://assetstore.unity.com/packages/3d/props/outdoor-pack-vol-1-87995");
			AddCreditsLine("Wooden Houses Set PBR", "ATOMICU3D", "https://assetstore.unity.com/packages/3d/environments/wooden-houses-set-pbr-95495");
			AddCreditsLine("Workbench With Tools", "Ieva Lickiene", "https://assetstore.unity.com/packages/3d/props/tools/workbench-with-tools-121021");
			AddCreditsLine("Rusty Black Shovel", "Sergi Nicolás", "https://assetstore.unity.com/packages/3d/props/tools/rusty-black-shovel-73088");
			AddCreditsLine("Bloody Riot Gear Bulletproof Vest Worn", "Turbosquid, Inc", "https://assetstore.unity.com/packages/3d/props/clothing/113731");
			AddCreditsLine("5 BackPacks (HQ PBR)", "Indie_G", "https://assetstore.unity.com/packages/3d/props/clothing/119747");
			AddCreditsLine("Frost Effect", "Steve Craeynest", "https://assetstore.unity.com/packages/tools/particles-effects/5337");
			AddCreditsLine("AKM", "Chirmandirkun", "https://assetstore.unity.com/packages/3d/props/guns/72911");
			AddCreditsLine("Rifle", "Game-Ready Studio", "https://assetstore.unity.com/packages/3d/props/guns/25668");
			AddCreditsLine("M40A3 Sniper Rifle", "Lemmolab", "https://assetstore.unity.com/packages/3d/props/weapons/107756");
			AddCreditsLine("1911 Pistol Pack", "Perfect Games", "https://assetstore.unity.com/packages/3d/props/guns/88439");
			AddCreditsLine("Hand Painted Shotgun", "OneProgram's", "https://assetstore.unity.com/packages/3d/props/guns/61481");
			AddCreditsLine("Rifle Crouch and Prone Pro", "Kubold", "https://assetstore.unity.com/packages/3d/animations/20380");
			AddCreditsLine("Farm Plants", "Dimaantipanov", "https://assetstore.unity.com/packages/3d/props/food/104354");
			AddCreditsLine("Campfire and Cooking Place", "Useful3D", "https://assetstore.unity.com/packages/3d/props/97013");
			AddCreditsLine("Rabbit", "Protofactor Inc", "https://assetstore.unity.com/packages/3d/characters/animals/4988");
			AddCreditsLine("Rifle Animset Pro", "Kubold", "https://assetstore.unity.com/packages/3d/animations/15098");
			AddCreditsLine("Archer Animset Pro", "Riko", "https://assetstore.unity.com/packages/3d/animations/89757");
			AddCreditsLine("Survival Animset Pro Vol 2", "Kubold", "https://assetstore.unity.com/packages/3d/animations/114412");
			AddCreditsLine("Survival Animset Pro Vol 2", "Kubold", "https://assetstore.unity.com/packages/3d/animations/survival-animset-pro-vol-1-112577");
			AddCreditsLine("Traditional Water Well", "3DMondra", "https://assetstore.unity.com/packages/3d/props/exterior/4477");
			AddCreditsLine("UMA Zombies Volume 1", "Will B", "https://assetstore.unity.com/packages/3d/characters/32353");
			AddCreditsLine("Modern Time: Mercenary Soldier", "Do Games", "https://assetstore.unity.com/packages/3d/characters/humanoids/modern-time-mercenary-soldier-29673");
			AddCreditsLine("UMA Face Pack Vol 1", "Will B", "https://assetstore.unity.com/packages/3d/characters/73428");
			AddCreditsLine("Watering Can", "WB-Gameart", "https://assetstore.unity.com/packages/3d/props/exterior/98176");
			AddCreditsLine("UMA 2 - Unity Multipurpose Avatar", "UMA Steering Group", "https://assetstore.unity.com/packages/3d/characters/35611");
			AddCreditsLine("Old School Playground Gear", "Ieva Likiene", "https://assetstore.unity.com/packages/3d/props/exterior/old-school-playground-gear-79692");
			AddCreditsLine("Chainlink Fences", "Kobra Game Studios", "https://assetstore.unity.com/packages/3d/chainlink-fences-73107");
			AddCreditsLine("Abandoned Diner", "Gamepoly", "https://assetstore.unity.com/packages/3d/props/exterior/50698");
			AddCreditsLine("Garbage and Trash Props", "Finward Studios", "https://assetstore.unity.com/packages/3d/props/industrial/74482");
			AddCreditsLine("Abandoned Props Pack PBR", "KK Design", "https://assetstore.unity.com/packages/3d/props/66781");
			AddCreditsLine("Abandoned Motel", "8Bull", "https://assetstore.unity.com/packages/3d/environments/urban/7349");
			AddCreditsLine("Abandoned Gas Station", "8Bull", "https://assetstore.unity.com/packages/3d/environments/urban/7808");
			AddCreditsLine("Realistic Sandbags", "FlamingSands", "https://assetstore.unity.com/packages/3d/props/exterior/95964");
			AddCreditsLine("Metal Painted Pitting Dirty", "Lex4Art", "https://assetstore.unity.com/packages/2d/textures-materials/metals/metal-painted-pitting-dirty-41509");
			AddCreditsLine("Metal 06", "CrazyTextures", "https://assetstore.unity.com/packages/2d/textures-materials/metals/23812");
			AddCreditsLine("Roofing 03", "CrazyTextures", "https://assetstore.unity.com/packages/2d/textures-materials/23333");
			AddCreditsLine("Air Conditioner HD", "Indie_G", "https://assetstore.unity.com/packages/3d/props/industrial/53466");
			AddCreditsLine("Glass Window 2", "RDR", "https://assetstore.unity.com/packages/2d/textures-materials/building/79119");
			AddCreditsLine("Ruined Car", "000734", "https://assetstore.unity.com/packages/3d/vehicles/5909");
			AddCreditsLine("Rusty Cars 2", "VisionGames", "https://assetstore.unity.com/packages/3d/vehicles/land/67096");
			AddCreditsLine("Rusty Car", "Gargore", "https://assetstore.unity.com/packages/3d/vehicles/land/5924");
			AddCreditsLine("Delapidated Car", "Ryan Skinner", "https://assetstore.unity.com/packages/3d/vehicles/land/5923");
			AddCreditsLine("Concrete Asphalt 02", "The Texture Lab", "https://assetstore.unity.com/packages/2d/textures-materials/roads/52433");
			AddCreditsLine("PBR Ground Materials #1 [Dirt & Grass]", "John's Junkyard Assets", "https://assetstore.unity.com/packages/2d/textures-materials/floors/85402");
			AddCreditsLine("Materials Sample Vol 1", "Mikołaj Spychał", "https://assetstore.unity.com/packages/2d/textures-materials/floors/39745");
			AddCreditsLine("Nature Package", "SilverTM", "https://assetstore.unity.com/packages/3d/vegetation/42225");
			AddCreditsLine("Dynamic Nature Starter", "NatureManufacture", "https://assetstore.unity.com/packages/3d/environments/79388");
			AddCreditsLine("Low Poly PBR Melee Weapon Pack", "Red Dot Lab", "https://assetstore.unity.com/packages/3d/props/weapons/67466");
			AddCreditsLine("Sword Animset Pro", "Kubold", "https://assetstore.unity.com/packages/3d/animations/38302");
			AddCreditsLine("ZOMBIE PRO: MoCap Animation Pack", "Mocap Online", "https://assetstore.unity.com/packages/3d/animations/47059");
			AddCreditsLine("UMA Hair Pack 1", "Will B", "https://assetstore.unity.com/packages/3d/characters/26521");
			AddCreditsLine("UMA 2 Military Militia", "Ruby Roid", "https://assetstore.unity.com/packages/3d/characters/69763");
			AddCreditsLine("UMA Long Coat", "Will B", "https://assetstore.unity.com/packages/3d/characters/36104");
			AddCreditsLine("AL Female Civilian Pack for UMA", "AlienLab", "https://assetstore.unity.com/packages/3d/characters/44869");
			AddCreditsLine("Free Hats for UMA", "Sliced Studio", "https://assetstore.unity.com/packages/3d/characters/33503");
			AddCreditsLine("Movement Animset Pro", "Kubold", "https://assetstore.unity.com/packages/3d/animations/14047");
			AddCreditsLine("Pistol Animset Pro", "Kubold", "https://assetstore.unity.com/packages/3d/animations/15828");
			AddCreditsLine("Rock 01", "CrazyTextures", "https://assetstore.unity.com/packages/2d/textures-materials/stone/rock-01-28673");
			AddCreditsLine("Dan Wesson Model 715", "MyNameIsVoo", "https://assetstore.unity.com/packages/3d/props/weapons/dan-wesson-model-715-72033");
			AddCreditsLine("FREE Snowman", "ANGRY MESH", "https://assetstore.unity.com/packages/3d/props/free-snowman-105123");
			AddCreditsLine("Helicopter pack 2.", "PolyLab", "https://assetstore.unity.com/packages/3d/vehicles/air/helicopter-pack-2-144496");
			AddCreditsLine("Bloody Riot Gear Leg Protector Worn", "TurboSquid, Inc.", "https://assetstore.unity.com/packages/3d/props/clothing/armor/bloody-riot-gear-leg-protector-worn-113733");
			AddCreditsLine("ANIMALS FULL PACK", "PROTOFACTOR, INC", "https://assetstore.unity.com/packages/3d/characters/animals/animals-full-pack-5032");
			AddCreditsLine("Volumetric Blood Fluids", "kripto289", "https://assetstore.unity.com/packages/vfx/particles/volumetric-blood-fluids-173863");
			AddCreditsLine("Skinned Mesh Combiner MT - Character Mesh Merge, Atlasing Support & More", "MT Assets", "https://assetstore.unity.com/packages/tools/game-toolkits/skinned-mesh-combiner-mt-character-mesh-merge-atlasing-support-m-135422");
			AddCreditsLine("Food Items - Proteins Pack", "Lockem Reality", "https://assetstore.unity.com/packages/3d/props/food/food-items-proteins-pack-118683");
			AddCreditsLine("Bullet Hole Decals", "Underground Workshop Corp", "https://assetstore.unity.com/packages/2d/textures-materials/bullet-hole-decals-183758");
			AddCreditsLine("Domestic birds pack", "Rifat_Bilalov", "https://assetstore.unity.com/packages/3d/characters/animals/birds/domestic-birds-pack-96679");
			AddCreditsLine("Farm Animal Sounds", "Cafofo", "https://assetstore.unity.com/packages/audio/sound-fx/animals/farm-animal-sounds-179750");
			AddCreditsLine("Old Rusted Bowl", "Game-Ready Studios", "https://assetstore.unity.com/packages/3d/props/electronics/old-rusted-bowl-24448");
			AddCreditsLine("Ultimate Sound FX Bundle", "Sidearm Studios", "https://assetstore.unity.com/packages/audio/sound-fx/ultimate-sound-fx-bundle-151756");
			AddCreditsLine("HQ Retro Farmhouse (Modular)", "NOT_Lonely", "https://assetstore.unity.com/packages/3d/environments/urban/hq-retro-farmhouse-modular-154929");
			AddCreditsLine("Low Poly Mail Boxes Pack", "AAAnimators", "https://assetstore.unity.com/packages/3d/environments/urban/low-poly-mail-boxes-pack-138772");
			AddCreditsLine("Concrete Bunker 03", "GamePoly", "https://assetstore.unity.com/packages/3d/concrete-bunker-03-23429");
			AddCreditsLine("Store Front Vol - 1,2,6 PBR", "DevDen", "https://assetstore.unity.com/packages/3d/environments/landscapes/store-front-vol-6-pbr-226228");
			AddCreditsLine("Small Town America - Streets", "MultiFlagStudios", "https://assetstore.unity.com/packages/3d/small-town-america-streets-free-59759");
			AddCreditsLine("AAA Quality - Road Barricades", "kawetofe", "https://assetstore.unity.com/packages/3d/props/aaa-quality-road-barricades-142191");
			AddCreditsLine("Vehicle - Essentials", "Nox_Sound", "https://assetstore.unity.com/packages/audio/sound-fx/transportation/vehicle-essentials-194951");
			AddCreditsLine("PBR Jerrycan Free", "DNK_DEV", "https://assetstore.unity.com/packages/3d/props/pbr-jerrycan-free-80011A");
			AddCreditsLine("Tactical Silencer .45 ACP", "FreakGames", "https://assetstore.unity.com/packages/3d/props/guns/tactical-silencer-45-acp-9mm-114262");
			AddCreditsLine("Grimoire Style Book", "Robotic Rainbow Studios", "https://assetstore.unity.com/packages/3d/props/grimoire-style-book-3996");
			AddCreditsSpacer();
			AddCreditsHeading("Vecteezy", "https://www.vecteezy.com/");
			AddCreditsLine("Abstract halftone dotted background", "Buntoon Rodseng", "https://www.vecteezy.com/vector-art/3464173-abstract-halftone-dotted-background-wave-vintage-layout");
			AddCreditsSpacer();
			AddCreditsHeading("freepnglogos", "https://www.freepnglogos.com/");
			AddCreditsLine("match stick", "John D.", "https://www.freepnglogos.com/images/stick-25050.html");
			AddCreditsSpacer();
			AddCreditsHeading("Textures.com", "https://www.textures.com/");
			AddCreditsSpacer();
			AddCreditsHeading("BlendSwap", "https://www.blendswap.com");
			AddCreditsLine("Low-Poly House", "hjmediastudios", "https://www.blendswap.com/blends/view/6447");
			AddCreditsSpacer();
			AddCreditsHeading("Everything Else", "");
			AddCreditsLine("Bob", "https://www.youtube.com/watch?v=dQw4w9WgXcQ");
			AddCreditsSpacer();
		}
	}

	public void AddCreditsHeading(string heading, string url)
	{
		CreditsLine item = new CreditsLine
		{
			Type = CreditsLine.LineType.Heading,
			Text = heading,
			Url = url
		};
		Lines.Add(item);
	}

	public void AddCreditsLine(string name, string url)
	{
		CreditsLine item = new CreditsLine
		{
			Type = CreditsLine.LineType.Single,
			Text = name,
			Url = url
		};
		Lines.Add(item);
	}

	public void AddCreditsLine(string role, string name, string url)
	{
		CreditsLine item = new CreditsLine
		{
			Type = CreditsLine.LineType.Double,
			Role = role,
			Text = name,
			Url = url
		};
		Lines.Add(item);
	}

	public void AddCreditsLineWithIcon(string role, string name, string url, Resource<Texture2D> icon)
	{
		CreditsLine item = new CreditsLine
		{
			Type = CreditsLine.LineType.Icon,
			Role = role,
			Text = name,
			Url = url,
			Icon = icon
		};
		Lines.Add(item);
	}

	public void AddCreditsSpacer()
	{
		CreditsLine item = new CreditsLine
		{
			Type = CreditsLine.LineType.Spacer
		};
		Lines.Add(item);
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		RectTransform rectTransform = (RectTransform)UnityContents.transform;
		if (!ManualScroll)
		{
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, yOffset);
			yOffset += Time.unscaledDeltaTime * AutoScrollSpeed;
			if (yOffset >= rectTransform.rect.height)
			{
				WantPop = true;
			}
		}
		if (AddedLineIndex >= Lines.Count - 1 || !(yOffset + HudBehaviour.Instance.HudPanelRectTransform.rect.height >= rectTransform.rect.height))
		{
			return;
		}
		CreditsLine line = Lines[AddedLineIndex];
		switch (line.Type)
		{
		case CreditsLine.LineType.Heading:
			Object.Instantiate(BaseMenu.CreditsHeading.GetAsset(), UnityContents.transform).GetComponent<TextMeshProUGUI>().SetUnityText(line.Text);
			break;
		case CreditsLine.LineType.Single:
		{
			GameObject gameObject2 = Object.Instantiate(BaseMenu.CreditsLineSingle.GetAsset(), UnityContents.transform);
			gameObject2.FindChild("Name").GetComponent<TextMeshProUGUI>().SetUnityText(line.Text);
			if (!string.IsNullOrEmpty(line.Url))
			{
				gameObject2.GetComponent<Button>().onClick.AddListener(delegate
				{
					Application.OpenURL(line.Url);
				});
				gameObject2.GetComponent<MenuSelectableBehaviour>().WantLinkyCursor = true;
			}
			else
			{
				gameObject2.GetComponent<Button>().interactable = false;
				gameObject2.FindChild("Link").GetComponent<RawImage>().color = MathUtil.TransparentBlackCol;
			}
			break;
		}
		case CreditsLine.LineType.Double:
		{
			GameObject gameObject3 = Object.Instantiate(BaseMenu.CreditsLineDouble.GetAsset(), UnityContents.transform);
			gameObject3.FindChild("Role").GetComponent<TextMeshProUGUI>().SetUnityText(line.Role);
			gameObject3.FindChild("Name").GetComponent<TextMeshProUGUI>().SetUnityText(line.Text);
			if (!string.IsNullOrEmpty(line.Url))
			{
				gameObject3.GetComponent<Button>().onClick.AddListener(delegate
				{
					Application.OpenURL(line.Url);
				});
				gameObject3.GetComponent<MenuSelectableBehaviour>().WantLinkyCursor = true;
			}
			else
			{
				gameObject3.GetComponent<Button>().interactable = false;
				gameObject3.FindChild("Link").GetComponent<RawImage>().color = MathUtil.TransparentBlackCol;
			}
			break;
		}
		case CreditsLine.LineType.Icon:
		{
			GameObject gameObject = Object.Instantiate(BaseMenu.CreditsLineIcon.GetAsset(), UnityContents.transform);
			gameObject.FindChild("Role").GetComponent<TextMeshProUGUI>().SetUnityText(line.Role);
			gameObject.FindChild("Name").GetComponent<TextMeshProUGUI>().SetUnityText(line.Text);
			gameObject.FindChild("Icon").GetComponent<RawImage>().texture = (Texture2D)line.Icon;
			if (!string.IsNullOrEmpty(line.Url))
			{
				gameObject.GetComponent<Button>().onClick.AddListener(delegate
				{
					Application.OpenURL(line.Url);
				});
				gameObject.GetComponent<MenuSelectableBehaviour>().WantLinkyCursor = true;
			}
			else
			{
				gameObject.GetComponent<Button>().interactable = false;
				gameObject.FindChild("Link").GetComponent<RawImage>().color = MathUtil.TransparentBlackCol;
			}
			break;
		}
		case CreditsLine.LineType.Spacer:
			Object.Instantiate(BaseMenu.CreditsSpacer.GetAsset(), UnityContents.transform);
			break;
		}
		AddedLineIndex++;
	}

	public override void PreHandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
		float axis = InputFunctionManager.Instance.GetAxis(InputFunction.MoveVert);
		if (axis != 0f)
		{
			RectTransform rectTransform = (RectTransform)UnityContents.transform;
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, yOffset);
			yOffset -= Time.unscaledDeltaTime * ManualScrollSpeed * axis;
			yOffset = Mathf.Clamp(yOffset, 0f - HudBehaviour.Instance.HudPanelRectTransform.rect.height, rectTransform.rect.height);
			ManualScroll = true;
		}
	}
}
