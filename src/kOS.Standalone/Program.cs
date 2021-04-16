using System;
using System.Text;
using kOS.Safe.Screen;
using kOS.Safe.UserIO;
using kOS.Safe.Utilities;
using kOS.Safe.Compilation;
using kOS.Safe.Serialization;

namespace kOS.Standalone
{
    class Program
    {
        static void Main(string[] args)
        {
            kOS.Safe.Utilities.SafeHouse.Init(
                new StandaloneConfig(),
                new Safe.Encapsulation.VersionInfo(0, 0, 0, 0),
                "http://ksp-kos.github.io/KOS_DOC/",
                Environment.NewLine == "\r\n",
                "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Kerbal Space Program\\Ships\\Script"
            );
            kOS.Safe.Utilities.SafeHouse.Logger = new DebugLogger();
            AssemblyWalkAttribute.Walk();
            Opcode.InitMachineCodeData();
            CompiledObject.InitTypeData();
            SafeSerializationMgr.CheckIDumperStatics();

            Console.TreatControlCAsInput = true;

            while (true)
            {
                Console.Clear();
                int lineOffset = 0;

                var shared = new StandaloneSharedObjects();
                kOS.Safe.Utilities.SafeHouse.Logger = shared.Logger;

                shared.VolumeMgr.SwitchTo(shared.VolumeMgr.GetVolume(0));

                shared.Screen.SetSize(Console.WindowHeight, Console.BufferWidth);
                IScreenSnapShot snapshot = ScreenSnapShot.EmptyScreen(shared.Screen);

                shared.Cpu.Boot();

                while (shared.ProcessorMode == Safe.Module.ProcessorModes.READY)
                {
                    if (shared.Screen.RowCount != Console.WindowHeight || shared.Screen.ColumnCount != Console.BufferWidth)
                        shared.Screen.SetSize(Console.WindowHeight, Console.BufferWidth);

                    Console.CursorVisible = shared.Interpreter.IsWaitingForCommand();
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        char mapped = key.KeyChar;
                        bool special = true;
                        switch (mapped) {
                        case '\u0003':
                            mapped = (char)UnicodeCommand.BREAK;
                            break;
                        default:
                            special = false;
                            break;
                        }
                        if (special) {
                            shared.Interpreter.SpecialKey(mapped);
                        } else if (shared.Interpreter.IsWaitingForCommand()) {
                            shared.Interpreter.Type(mapped);
                        } else {
                            shared.Screen.CharInputQueue.Enqueue(mapped);
                        }
                    }

                    var oldSnapshot = snapshot;
                    snapshot = new ScreenSnapShot(shared.Screen).DeepCopy();
                    var diff = snapshot.DiffFrom(oldSnapshot);

                    int i = 0;
                    while (i < diff.Length)
                    {
                        char c = diff[i];

                        switch (c)
                        {
                            case (char)UnicodeCommand.CLEARSCREEN:
                                Console.Clear();
                                i++;
                                break;
                            case (char)UnicodeCommand.TITLEBEGIN:
                                i++;
                                var titleBuilder = new StringBuilder();

                                while (diff[i] != (char)UnicodeCommand.TITLEEND)
                                {
                                    titleBuilder.Append(diff[i]);
                                    i++;
                                }
                                Console.Title = titleBuilder.ToString();
                                i++;
                                break;
                            case (char)UnicodeCommand.TELEPORTCURSOR:
                                i++;
                                Console.CursorLeft = (int)diff[i++];
                                Console.CursorTop = (int)diff[i++] + lineOffset;
                                break;
                            case (char)UnicodeCommand.BEEP:
                                i++;
                                Console.Beep();
                                break;
                            case (char)UnicodeCommand.SCROLLSCREENUPONE:
                                Console.CursorTop++;
                                lineOffset++;
                                i++;
                                break;
                            case (char)UnicodeCommand.SCROLLSCREENDOWNONE:
                                Console.CursorTop--;
                                lineOffset--;
                                i++;
                                break;
                            default:
                                if (c > 255 && c != 0x2588)
                                    System.Diagnostics.Debug.WriteLine("Unknown: " + c);
                                Console.Write(c);
                                i++;
                                break;
                        }
                    }

                    shared.UpdateHandler.UpdateObservers(0.05);
                    shared.UpdateHandler.UpdateFixedObservers(0.05);
                }
                if (shared.ProcessorMode == Safe.Module.ProcessorModes.OFF)
                    break;
            }
        }
    }
}
