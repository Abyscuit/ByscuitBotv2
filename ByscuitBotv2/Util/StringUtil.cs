﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ByscuitBotv2.Util
{
    public static class StringUtil
    {
        
        private static Random _random = new Random();

        private static List<string> _kisakQuotes = new List<string>
        {
            // Kisak gets welcomed to the CS:GO team - Life of Kisak Part I
            "​​​​Hello i'm kisak-valve!",
            "I'm the github moderator of volvo  softwar.",
            "Here's my github: github.com/kisak-valve.",
            "And here's my story:",
            "Hey boy, come here I said to him.",
            "Y-y-yes boss?",
            "he replied.",
            "I hear you wanted to be moved over here into the CS:GO section",
            "T-that's right..",
            "His voice cracked at the end of each sentence.",
            "What a beta I thought to myself.",
            "Well anyway, here's your big chance, kissakk, you need to port this here Accept Button shit over to Linux.",
            "You know Linux right?",
            "Yes, I installed gentoo, bazinga",
            "Uhh. Ok. Well get to work, if you need help just skid off doom 3, that runs on Linux right? Nevermind.",
            "Get out of my office before I call security",
            "Right away Sir!",
            "I got the code back a couple weeks later and after another 3 months we were able to get it compile.",
            "This is utter shit kisssassk, where did you learn to program?",
            "From Devry Univerisity sir!",
            "Well shit, head over to learncpp and finish all their lessons and come talk to me again regarding your CS:GO position.",
            "In the meantime, you can be my github-slave.",
            "Gee boss, you really mean it? Thanks a lot!",
            "Let me call my BF and give him the news",
            "No time, kissass, we gotta head over into the recording studio to meet with Soulja Boy for the new half-life 3 OST.",
            "I want you to get back to work ASAP.",
            "Alright.",
            
            // Kisak has to deal with cheaters on GitHub - Life of Kisak Part II
            "So I was sitting my Office and in comes Kissass..",
            "Oh boy I thought to myself.",
            "Uhhh... Excuse me...., Kisak whimpered.",
            "I ignored him, hoping he would go away.",
            "Tycho... I need help....",
            "I pretended to look busy on my computer, I was really playing BloonsTD waiting to head over to the snackbar in an hour.",
            "There's a github issue I can't solve...",
            "PLEASEEE He said uncomfortably loud.",
            "A couple of the other VAC members looked up at the outburst of autism.",
            "Okay Kisak, you've got 15 seconds, what do you want?",
            "The cheaters!",
            "They're back on github!",
            "Ok... Have you tried banning them?",
            "No, they said that they're getting VAC'd for their username",
            "He was talking about the fix I did for those pesky catbots.",
            "Those dumbass freetards just used catbot as the name for all their linux accounts so I just BTFO'd them.",
            "Yes Kisak, they're cheating.",
            "Just tell them that we never reverse VAC bans.",
            "uhh...",
            "We're done here, Kisak, go back to the TF2 department.",
            "I said.",
            "Yess, sirr!",
            "Thank you!",
            "They pester me for 6 months to fix the catbots and I finally do and Kisak does this shit to me?",
            "Where did they find this guy?",
            "Ok, snack-time boys I said to the VAC squad.",
            "Down at the snackbar I got myself my favorite treat, a pink cookie, and a large diet Coke.",
            "Gotta watch my figure",
            "I sat down at the Dota 2 Themed Brewmaster table and started to eat off of his furry belly.",
            "I pulled out my phone and started to browse le reddit when I saw",
            "/r/Linux_Gaming",
            "Valve will VAC ban you automatically for having catbot in your linux username",
            "Jesus Christ, Kisak",
            "I ran back up stairs and went straight into the janitor's closet and TF2 dept.",
            "Kisak, what the hell did you do?",
            "We're all over reddit!",
            "Uhhh.... I just put that it was intentional that we're banning users with catbot in their name",
            "You dumbass, who told you that?",
            "That was just a temporary fix",
            "You're fired Kisak, get your stuff and get outta here",
            "It was too late though, we were all over pcgamer, hackernews, and even the steam news bot picked up the story.",
            "I need to do some kind of damage control",
            "I thought to myself.",
            "Just then my phone went off. It was from Gabe...",
            "Tycho, come to my office immediately bitch.",
            "And bring the lube",
            
            // Time for revenge - Life of Kisak Part III (Tycho perspective)
            "My anus was still sore from the last Gaben meeting.",
            "Pretty much everyone had left the office for the night, but I was stuck here working on VAC, trying to ban those damn free-tard cheaters.",
            "At the very least, catbots at least boost the tf2 player count, I thought to myself.",
            "I had a stackoverflow article open about ptrace, but I couldn't focus.",
            "I needed a break.",
            "I grabbed my Mouth-Fedora 6000 and pina-colada e-juice and headed out to elevator to the parking garage.",
            "*PFFFFFFFFFFFFFFFFFF*",
            "I ripped a huge cloud and blew it into the parking garage.",
            "The cloud faded and I saw a familiar vehicle...",
            "It was a rusty white toyota AE-86 with a Gentoo sticker on the back...",
            "Kisak?",
            "I said outloud.",
            "What was that dipshit doing here?",
            "He was fired weeks ago.",
            "I walked over to the car, it was empty.",
            "I put my hand on the hood, still warm.",
            "I must of just missed him on the elevator.",
            "I flicked out my Flip-Knife Fade and drove it into his front tire.",
            "As the air let out, I saw the Stat-trak counter go up by 1.",
            "I put my knife away and headed back up the elevator.",
            "The office was dark, we weren't allowed to turn on the lights after-hours to save money.",
            "I felt the wall on the right side around to my office and sat back down to my desk.",
            "I pressed F5 on the OpenVAC github repo one of those free-tards made to mock me.",
            "Hmmm... What does the grep command do again? I went back to google to look it up.",
            "All of a sudden, there was a blood-curdling scream from upstairs.",
            "It's probably just the Dota team making SFX for the new Pudge Arcana, I thought.",
            "I had to do a captcha because of too many searches.",
            "Oh yeah it's Linux shit, Grep... G-rep.",
            "Yeah.. Yeah.. G-man representin' -- G-Rep!",
            "I chuckled, that one was pretty good.",
            "I opened my e-mail client and started an e-mail to the writing team for half-life 3.",
            "Check out this sick joke guys, only Linux nerds will get it xD and Sent.",
            "I turned my chair 180 degrees and leaned back gazing at the bellevue night scene.",
            "A Magnificent view of the concrete jungle, 3 starbucks, a macy's, and ye olde vitamin shoppe.",
            "I could see a couple bums and crackheads staggering around, perhaps i'll give them a handout after work, I am just too privileged.",
            "I turned back around to my monitors when I heard, Tycho, you bitch.",
            "Kisak? Where are you?",
            "I'm right here you dick",
            "I moved one of my monitors to the left and saw him standing there. Kisak was not a tall man.",
            "What are you doing here, Kisak? You were fired two weeks ago.",
            "I can't take it anymore, this world is an illusion! He proclaimed.",
            "He pulled out an AK from under his black trench coat and pointed it up at me.",
            "Woah woah, Kisak, buddy, we can talk this out.",
            "No we can't! My life is ruined! This was all I had",
            "Comon Kisak, you know this wouldn't last, you weren't even paid. And you still have your e-flags or c-flags or whatever on your jentoo that you were bragging about.",
            "Tycho, it takes me hours to compile Firefox, god forbid, KDE or Gnome.",
            "Okay Kisak. But why do you use Gentoo then?",
            "Because it let us spend more time together. If anyone was wondering why I was wondering around all the time, I would just say that my code was compiling and it was true.",
            "...",
            "But those times are over now, Tycho. And now it's time for us to go.",
            "He aimed the rifle up.",
            "Kisak! Don't do this, comon we're pals",
            "*BOOM BOOM BOOM*",
            "Kisak fired 3 shots into my chest, I fell out of my chair.",
            "I couldn't breathe, my chest was physically destroyed, I felt the darkness closing in.",
            "But then, a yellow light started to faintly reflect off of the ceiling.",
            "It grew stronger and filled the room with its light.",
            "What? Kisak said.",
            "The light traveled to me and fully enveloped me, it pushed me into a standing position. I could feel my wounds healing.",
            "I looked to the source of the light and it was on my Desk, the 2017 Collector's Aegis of the Immortals.",
            "Kisak was rampant.",
            "What the Shit?!! It actually works??? I didn't have the money to level up my compendium that far!!!",
            "He paused for a minute, his face turned red.",
            "THIS ISN'T FAIR!",
            "I felt fully restored, standing, the light faded as quickly as it came and the Aegis burst into dust.",
            "Kisak stared at it and then smiled",
            "You only had one, right Tycho?",
            "He was right, I did only have one, I had to make a break for it.",
            "Kisak's AK was jammed after the 3 shots, he was fumbling with it trying to reload.",
            "There were police sirens in the distance.",
            "I could hear rustling in the next room.",
            "Kisak finally got the magazine in and aimed it at me, I had no time left.",
            "Thanks for standing still, ganker, he remarked.",
            "*CLUNK*",
            "Kisak was hit on the head with a crowbar from behind. He fell to the ground.",
            "*CLUNK* *CLUNK* *CLUNK* *CLUNK* *CLUNK* *CLUNK*",
            "I swinted through the darkness it looked like Jess.",
            "Jess!",
            "The police sirens were close now.",
            "Jess, thank god you came",
            "No time Tycho, we gotta move",
            "He walked over to the light of my monitors and I saw it was Jess, he was carrying a prepubescent girl under each arm.",
            "He then punched out the glass behind where I had been shot and pulled out a Portal gun",
            "One sec he said.",
            "He leaned on my desk and carefully aimed the portal gun at a residence across town, approximately 10 miles away.",
            "*PFEWWW*",
            "he shot at the ground and dropped one of the girls into the portal.",
            "That thing works Jess? What?",
            "He smirked and pulled out a compass and sextant.",
            "This one's a little more tricky, all the way back to Canada",
            "The portal gun has bullet drop? I asked him.",
            "Oh yeah, do you actually play any of our games? He asked back.",
            "Ehh, not really",
            "Got it! he said and fired into the night sky.",
            "He glanced at his watch for about 10 seconds, then dropped the other girl into the portal.",
            "Our turn, let's go! He fired a shot into the nearest StarBucks, grabbed my wrist and jumped us both down into the portal.",
            "We teleported directly into Starbucks, the barista gave us a weird look, and Jess disabled the portal.",
            "He turned to me, That, ladies and gentlemen, was Portal #3.",
            "We both started to laugh.",
            
            // What a panoramic view - Life of Kisak Part IV
            "*slurrrrrpppppp*",
            "\"Ahhh, that's good stuff.\", Kisak said to himself as he browsed the csgo perforce repo on his thinkpad.",
            "He alt-tabbed to Visual Studio and clicked the build button one more time.",
            "\"Oops almost forgot!\" He switched the dropdown box from Debug to Release and chuckled to himself.",
            "*sipppp, slurrrppppp, *smacks lips* sippppp*",
            "Kisak decided to call up the boss on skype and let him know the good news.",
            "He turned on his webcam and started the call.",
            "After about 5 rings, the video call connected and a man in prison stripes answered behind the bars of his cell.",
            "\"Good morning, Jess, I've got the panorama UI done.\"",
            "Jess was wiping red stuff off of his lips.",
            "Uhh is this a bad time?",
            "\"N-no, it's fine\" Jess replied.",
            "A large black male stirred in the bed in the background.",
            "\"Anyway, we're ready to ship, I built it in release-mode.\"",
            "\"Ok, just do it.\"",
            "Kisak moved his mouse over to his epic .bat script he made that automatically ships updates for him.",
            "and double clicked.",
            "\"Wait Kisak, don't forget about the Linux and mac builds.\" Jess said.",
            "Kisak\'s heart skipped a beat.",
            "\"I gotta go Kisak, don't forget about them, you hear me?\"",
            "\"Y-y-yes! Boss!\"",
            "The webcam went black and the call ended.",
            "Kisak franticly shut off his thinkpad.",
            "\"I gotta get into Gentoo!\"",
            "He pressed ESC as fast as he could as the old thinkpad slowly stirred to life for the 10000000x time.",
            "Oh shit which one is it?",
            "There was a long list of autistic distros he had multi-booted with his Windows installation.",
            "\"void\", \"arch scrubz\", \"gahnoo\", \"rinux\", \"backtrack5-blacked-edition\", \"tails(epic)\"",
            "He tried gahnoo and the gentoo logo came up.",
            "\"YES!!\" he screeched.", 
            "A few members of the Dota team looked over at his autistic outburst.",
            "Kisak was the only one left working on CS:GO and was alone in the CS:GO room after Jess had been jailed. Tycho went to the TF2 to get away from Kisak.",
            "He reached into his pocket and pulled out his fingerless gloves and put them on.",
            "\"Let\'s rock.\"",
            "He was prompted with a CLI login,",
            "Username: kisak",
            "Password: ********************************************************************",
            "He typed out the pseudo-randomly generated passphrase as fast as he could.",
            "Sorry, try again.",
            "Password: **********************************************************************",
            "He tried it again and got his Gentoo Banner.",
            "Ok now to start X. [kisak@gentoo>you]$ startx",
            "Xorg config errors flew all over the screen.",
            "\"Shit!\"",
            "Just then he remembered he fixed this by reinstalling sabayon and quickly pulled the battery out of his thinkpad.",
            "He spammed ESC again as the thinkpad whined, sputtered, stirred, and finally the hard drive started clicking.",
            "Ok I think it's rinux",
            "He pressed enter on \"rinux\"", 
            "It started to boot...",
            "LiLo VFS: Cannot open root device \"803\" or 08:03",
            "Please append a correct \"root=\" boot option",
            "Kernel panic: VFS: Unable to mount root fs on 08:03",
            "All of Kisak's linux's were broken.",
            "This looked to be the end for panoramic UI for linux.",
            "Kisak had gone too far, he couldn't be fired now.",
            "He went back into windows, got on putty, and ssh'd into the csgo web server.",
            "Used the lynx browser to login to the wordpress admin",
            "And quickly edited the update page to show Windows-Only.",
            "This should buy him enough time.",
            "He quickly went on freelancer and started a new job offering. \"Port application over to Linux\"",
            "Almost immediately he got a PM from a Mr. Pajeet saying \"Sir, I think we may have deal now\"",
            "Kisak Grinned and took another sip of his coffee."
        };
        
        public static string Reverse(this string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }

        public static string GetRandomKisakQuote()
        {
            return _kisakQuotes[_random.Next(0, _kisakQuotes.Count - 1)];
        }

    }
}
