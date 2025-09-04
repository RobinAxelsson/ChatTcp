using ChatTcp.Cli.Presentation;

namespace ChatTcp.Cli;
internal static class Program
{

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Started");

        var cts = new CancellationTokenSource();

        while (!cts.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                MainControlFlow(cts);
            }
        }

        Console.WriteLine("Exiting...");
    }

    private static int selectedLine = 0;
    private static void MainControlFlow(CancellationTokenSource cts)
    {
        var keyInfo = Console.ReadKey(true);
        if (keyInfo.Key == ConsoleKey.Escape)
        {
            cts.Cancel();
        }
        if (keyInfo.Key == ConsoleKey.D1 && keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            ConsoleWriter.Instance.WriteText(Text.LoremIpsum20Lines, 0);
            //set network state
            //request render if new state
            return;
        }
        if (keyInfo.Key == ConsoleKey.D2 && keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            //var lines = Text.AsciiTable.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            //int lineNumber = 0;

            for (int i = 0; i < 12000; i++)
            {
                ConsoleWriter.Instance.WriteText(i.ToString(), i);
            }

            //set chatView state
            //request render if new state
            return;
        }
        if (keyInfo.Key == ConsoleKey.UpArrow)
        {
            selectedLine++;
            ConsoleWriter.Instance.WriteText(selectedLine.ToString(), 0);
        }
        if (keyInfo.Key == ConsoleKey.DownArrow)
        {
            selectedLine--;
            ConsoleWriter.Instance.WriteText(selectedLine.ToString(), 0);
        }
        if (keyInfo.Key == ConsoleKey.L)
        {
            ConsoleWriter.Instance.ClearLine(selectedLine);
        }
        if (keyInfo.Key == ConsoleKey.H)
        {
            ConsoleWriter.Instance.WriteText("hello world", selectedLine);
        }
        if (keyInfo.Key == ConsoleKey.Enter)
        {
            Console.Clear();
        }

        //send key to application flows
    }
}

public static class Text
{
    public static string CodeComment = @"//var cts = new CancellationTokenSource();
        //using var networkManager = new NetworkView();
        //var consoleChat = new ConsoleChat();

        //networkManager.OnPacketReceivedFromServer = consoleChat.ReceiveServerPacket;
        //consoleChat.SendChatMessage = networkManager.SendChatMessageToServer;

        //var serverTask = networkManager.Start(cts, port: port);
        //var consoleTask = consoleChat.Start(cts.Token);

        //try
        //{
        //    await await Task.WhenAny(serverTask, consoleTask);
        //}
        //finally
        //{
        //    cts.Cancel();
        //    await Task.WhenAll(serverTask, consoleTask);
        //}
";

    public static string LoremIpsum20Lines =
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit.
Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.
Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
Integer nec odio. Praesent libero. Sed cursus ante dapibus diam.
Sed nisi. Nulla quis sem at nibh elementum imperdiet.
Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta.
Mauris massa. Vestibulum lacinia arcu eget nulla.
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos.
Curabitur sodales ligula in libero.
Sed dignissim lacinia nunc.
Curabitur tortor. Pellentesque nibh.
Aenean quam. In scelerisque sem at dolor.
Maecenas mattis. Sed convallis tristique sem.
Proin ut ligula vel nunc egestas porttitor.
Morbi lectus risus, iaculis vel, suscipit quis, luctus non, massa.
Fusce ac turpis quis ligula lacinia aliquet.
Mauris ipsum. Nulla metus metus, ullamcorper vel, tincidunt sed, euismod in, nibh.
Quisque volutpat condimentum velit.";

    public static readonly string OneToHundredWords =
@"one, two, three, four, five, six, seven, eight, nine, ten,
eleven, twelve, thirteen, fourteen, fifteen, sixteen, seventeen, eighteen, nineteen, twenty,
twenty-one, twenty-two, twenty-three, twenty-four, twenty-five, twenty-six, twenty-seven, twenty-eight, twenty-nine, thirty,
thirty-one, thirty-two, thirty-three, thirty-four, thirty-five, thirty-six, thirty-seven, thirty-eight, thirty-nine, forty,
forty-one, forty-two, forty-three, forty-four, forty-five, forty-six, forty-seven, forty-eight, forty-nine, fifty,
fifty-one, fifty-two, fifty-three, fifty-four, fifty-five, fifty-six, fifty-seven, fifty-eight, fifty-nine, sixty,
sixty-one, sixty-two, sixty-three, sixty-four, sixty-five, sixty-six, sixty-seven, sixty-eight, sixty-nine, seventy,
seventy-one, seventy-two, seventy-three, seventy-four, seventy-five, seventy-six, seventy-seven, seventy-eight, seventy-nine, eighty,
eighty-one, eighty-two, eighty-three, eighty-four, eighty-five, eighty-six, eighty-seven, eighty-eight, eighty-nine, ninety,
ninety-one, ninety-two, ninety-three, ninety-four, ninety-five, ninety-six, ninety-seven, ninety-eight, ninety-nine, one hundred.";

    public static string NoteAcceptOp =
@"I kept your readonly on fields in AcceptOp. That means they must be assigned in the constructor and cannot be reassigned later.

In ReceiveOp and SendOp, you had required initializers — those are now covered by the constructor, so required is not needed anymore unless you still want object initializer syntax as an option.

Since these are record structs, your constructor coexists with the compiler-generated value equality and ToString.";

    public static readonly string AsciiTable =
@"<NUL> 0 0x00 NUL (null)
<SOH> 1 0x01 SOH (start of heading)
<STX> 2 0x02 STX (start of text)
<ETX> 3 0x03 ETX (end of text)
<EOT> 4 0x04 EOT (end of transmission)
<ENQ  > 5 0x05 ENQ (enquiry)
<ACK> 6 0x06 ACK (acknowledge)
<BEL> 7 0x07 BEL (bell)
<BS > 8 0x08 BS (backspace)
<TAB> 9 0x09 TAB (horizontal tab)
<LF > 10 0x0A LF (line feed)
<VT > 11 0x0B VT (vertical tab)
<FF > 12 0x0C FF (form feed)
<CR > 13 0x0D CR (carriage return)
<SO > 14 0x0E SO (shift out)
<SI > 15 0x0F SI (shift in)
<DLE> 16 0x10 DLE (data link escape)
<DC1> 17 0x11 DC1 (device control 1)
<DC2> 18 0x12 DC2 (device control 2)
<DC3> 19 0x13 DC3 (device control 3)
<DC4> 20 0x14 DC4 (device control 4)
<NAK> 21 0x15 NAK (negative acknowledge)
<SYN> 22 0x16 SYN (synchronous idle)
<ETB> 23 0x17 ETB (end of transmission block)
<CAN> 24 0x18 CAN (cancel)
<EM > 25 0x19 EM (end of medium)
<SUB> 26 0x1A SUB (substitute)
<ESC> 27 0x1B ESC (escape)
<FS > 28 0x1C FS (file separator)
<GS > 29 0x1D GS (group separator)
<RS > 30 0x1E RS (record separator)
<US > 31 0x1F US (unit separator)
<SP > 32 0x20 Space
! 33 0x21 Exclamation mark
"" 34 0x22 Quotation mark
# 35 0x23 Number sign
$ 36 0x24 Dollar sign
% 37 0x25 Percent sign
& 38 0x26 Ampersand
' 39 0x27 Apostrophe
( 40 0x28 Left parenthesis
) 41 0x29 Right parenthesis
* 42 0x2A Asterisk
+ 43 0x2B Plus sign
, 44 0x2C Comma
- 45 0x2D Hyphen-minus
. 46 0x2E Full stop (period)
/ 47 0x2F Solidus (slash)
0 48 0x30 Digit zero
1 49 0x31 Digit one
2 50 0x32 Digit two
3 51 0x33 Digit three
4 52 0x34 Digit four
5 53 0x35 Digit five
6 54 0x36 Digit six
7 55 0x37 Digit seven
8 56 0x38 Digit eight
9 57 0x39 Digit nine
: 58 0x3A Colon
; 59 0x3B Semicolon
< 60 0x3C Less-than sign
= 61 0x3D Equals sign
> 62 0x3E Greater-than sign
? 63 0x3F Question mark
@ 64 0x40 Commercial at
A 65 0x41 Latin capital letter A
B 66 0x42 Latin capital letter B
C 67 0x43 Latin capital letter C
D 68 0x44 Latin capital letter D
E 69 0x45 Latin capital letter E
F 70 0x46 Latin capital letter F
G 71 0x47 Latin capital letter G
H 72 0x48 Latin capital letter H
I 73 0x49 Latin capital letter I
J 74 0x4A Latin capital letter J
K 75 0x4B Latin capital letter K
L 76 0x4C Latin capital letter L
M 77 0x4D Latin capital letter M
N 78 0x4E Latin capital letter N
O 79 0x4F Latin capital letter O
P 80 0x50 Latin capital letter P
Q 81 0x51 Latin capital letter Q
R 82 0x52 Latin capital letter R
S 83 0x53 Latin capital letter S
T 84 0x54 Latin capital letter T
U 85 0x55 Latin capital letter U
V 86 0x56 Latin capital letter V
W 87 0x57 Latin capital letter W
X 88 0x58 Latin capital letter X
Y 89 0x59 Latin capital letter Y
Z 90 0x5A Latin capital letter Z
[ 91 0x5B Left square bracket
\ 92 0x5C Reverse solidus (backslash)
] 93 0x5D Right square bracket
^ 94 0x5E Circumflex accent
_ 95 0x5F Low line (underscore)
` 96 0x60 Grave accent
a 97 0x61 Latin small letter a
b 98 0x62 Latin small letter b
c 99 0x63 Latin small letter c
d 100 0x64 Latin small letter d
e 101 0x65 Latin small letter e
f 102 0x66 Latin small letter f
g 103 0x67 Latin small letter g
h 104 0x68 Latin small letter h
y 105 0x69 Latin small letter y
j 106 0x6A Latin small letter j
k 107 0x6B Latin small letter k
l 108 0x6C Latin small letter l
m 109 0x6D Latin small letter m
n 110 0x6E Latin small letter n
o 111 0x6F Latin small letter o
p 112 0x70 Latin small letter p
q 113 0x71 Latin small letter q
r 114 0x72 Latin small letter r
s 115 0x73 Latin small letter s
t 116 0x74 Latin small letter t
u 117 0x75 Latin small letter u
v 118 0x76 Latin small letter v
w 119 0x77 Latin small letter w
x 120 0x78 Latin small letter x
y 121 0x79 Latin small letter y
z 122 0x7A Latin small letter z
{ 123 0x7B Left curly bracket
| 124 0x7C Vertical bar
} 125 0x7D Right curly bracket
~ 126 0x7E Tilde
<DEL> 127 0x7F DEL (delete)";
}
