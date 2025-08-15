using process;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Management;
using System.Reflection;

namespace process
{
    public class Program
    {
        [Flags]
        public enum ListColumnType
        {
            Name = 1238848,
            Title = 1291012949,
            PID = 14385828,
            Handle = 184855952,
            StartTime = 195951219,
            Responding = 12838188,
            CommandLine = 38458181
        }

        public static void Main(string[] args)
        {
            var rootCmd = new RootCommand("Manages Windows processes.");

            var listCmd = new Command("list", "List the processes on the system.");

            var listCmdCols = new Option<ListColumnType>("SELECT", "SEL")
            {
                Description = "Specify the columns to display, separated by commas.",
                Arity = ArgumentArity.ZeroOrOne,
                CustomParser = ListColumnType (result) =>
                {
                    var strt = result.Tokens.FirstOrDefault();

                    if (strt != null)
                    {
                        ListColumnType? val = null;

                        if (strt.Value.Equals("*"))
                        {
                            val = ListColumnType.Name | ListColumnType.Title |
                            ListColumnType.PID | ListColumnType.Handle |
                            ListColumnType.StartTime | ListColumnType.Responding |
                            ListColumnType.CommandLine;
                        }
                        else
                        {
                            var sp = strt.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                            foreach (var s in sp)
                            {
                                if (Enum.TryParse<ListColumnType>(s, true, out var t))
                                {
                                    if (val != null)
                                    {
                                        val |= t;
                                    }
                                    else
                                    {
                                        val = t;
                                    }
                                }
                                else
                                {
                                    result.AddError("Not a valid column name: " + s);
                                }
                            }
                        }


                        if (val != null)
                        {
                            return (ListColumnType)val;
                        }
                    }

                    return ListColumnType.Name | ListColumnType.Title | ListColumnType.PID | ListColumnType.StartTime | ListColumnType.Responding;
                },
                DefaultValueFactory = ListColumnType (result) =>
                {
                    return ListColumnType.Name | ListColumnType.Title | ListColumnType.PID | ListColumnType.StartTime | ListColumnType.Responding;
                },
                
            };

            listCmd.Options.Add(listCmdCols);

            var listOnlyWindowed = new Option<bool>("--windowed", "-w")
            {
                Description = "Only show processes with windows."
            };

            listCmd.Options.Add(listOnlyWindowed);

            var killCmd = new Command("kill", "Kill a process.");
            var killPid = new Argument<int>("PID");

            var killPidCmd = new Command("pid", "Kill a process by PID.")
            {
                killPid
            };

            var killForce = new Option<bool>("--force", "-f")
            {
                Description = "Forcefully terminate the process."
            };

            var killTree = new Option<bool>("--tree", "-t")
            {
                Description = "Terminate the entire process tree."
            };

            killCmd.Options.Add(killForce);
            killCmd.Options.Add(killTree);
            killCmd.Validators.Add(result =>
            {
                if (result.GetValue(killTree) && !result.GetValue(killForce))
                {
                    result.AddError("--tree or -t must be used with --force or -f.");
                }
            });

            var killName = new Argument<string>("NAME");

            var killNameCmd = new Command("name", "Kill a process by name.")
            {
                killName
            };

            var machine = new Option<string>("--machine", "-m")
            {
                Description = "Specify the computer name to manage processes on.",
                DefaultValueFactory = string (result) =>
                {
                    return ".";
                }
            };

            rootCmd.Subcommands.Add(listCmd);
            rootCmd.Subcommands.Add(killCmd);

            killCmd.Subcommands.Add(killPidCmd);
            killCmd.Subcommands.Add(killNameCmd);

            rootCmd.Options.Add(machine);

            killNameCmd.SetAction(result =>
            {
                var process = Process.GetProcesses(result.GetValue(machine)!).FirstOrDefault(x =>
                {
                    try
                    {
                        if (x.ProcessName.Equals(result.GetValue(killName), StringComparison.OrdinalIgnoreCase)
                        || (Path.GetFileName(x.MainModule?.FileName)?.Equals(result.GetValue(killName), StringComparison.OrdinalIgnoreCase) ?? false)
                        || (Path.GetFileNameWithoutExtension(x.MainModule?.FileName)?.Equals(result.GetValue(killName), StringComparison.OrdinalIgnoreCase) ?? false)
                        || (x.MainModule?.FileName.Equals(result.GetValue(killName), StringComparison.OrdinalIgnoreCase) ?? false))
                        {
                            return true;
                        }
                    }
                    catch
                    {

                    }


                    return false;
                });

                if (process != null)
                {
                    try
                    {
                        if (!result.GetValue(killForce))
                        {
                            if (process.CloseMainWindow())
                            {
                                Console.WriteLine($"The process {process.GetDisplayName()} ({process.Id}) was terminated successfully.");
                            }
                            else
                            {
                                Console.WriteLine($"Killing process {process.GetDisplayName()} ({process.Id}) failed.");
                            }
                        }
                        else
                        {
                            process.Kill(result.GetValue(killTree));
                            Console.WriteLine($"The process {process.GetDisplayName()} ({process.Id}) was forcefully terminated.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Killing process {process.GetDisplayName()} ({process.Id}) failed: ({ex.HResult:X}) {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"A process for {result.GetValue(killName)} wasn't found. Try again later.");
                }
            });

            killPidCmd.SetAction(result =>
            {
                var process = Process.GetProcessById(result.GetValue(killPid), result.GetValue(machine)!);

                if (process != null)
                {
                    try
                    {
                        if (!result.GetValue(killForce))
                        {
                            if (process.CloseMainWindow())
                            {
                                Console.WriteLine($"The process {process.GetDisplayName()} ({process.Id}) was terminated successfully.");
                            }
                            else
                            {
                                Console.WriteLine($"Killing process {process.GetDisplayName()} ({process.Id}) failed.");
                            }
                        }
                        else
                        {
                            process.Kill(result.GetValue(killTree));
                            Console.WriteLine($"The process {process.GetDisplayName()} ({process.Id}) was forcefully terminated.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Killing process {process.GetDisplayName()} ({process.Id}) failed: ({ex.HResult:X}) {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"A process for PID {result.GetValue(killPid)} wasn't found. Try again later.");
                }
            });

            listCmd.SetAction(result =>
            {
                var table = new Table();

                var cols = result.GetValue(listCmdCols);

                table.Title("Processes");

                var columnList = new List<ListColumnType>();

                int i = 0;
                foreach (var col in Enum.GetValues<ListColumnType>())
                {
                    if (cols.HasFlag(col))
                    {
                        var n = col switch
                        {
                            ListColumnType.Name => "Name",
                            ListColumnType.Title => "Title",
                            ListColumnType.PID => "PID",
                            ListColumnType.Handle => "Handle",
                            ListColumnType.StartTime => "Start Time",
                            ListColumnType.Responding => "Responding",
                            ListColumnType.CommandLine => "Command Line",
                            _ => string.Empty
                        };

                        var c = new TableColumn(n);

                        if (i > 0)
                            switch (col)
                            {
                                case ListColumnType.Name:
                                case ListColumnType.CommandLine:
                                case ListColumnType.Title:
                                    table.AddColumn(c);
                                    break;
                                default:
                                    table.AddColumn(c.Centered());
                                    break;
                            }
                            
                        else
                            table.AddColumn(c);

                        columnList.Add(col);

                        i++;
                    }
                }

                foreach (var process in Process.GetProcesses(result.GetValue(machine)!))
                {
                    try
                    {

                        //if (!process.IsPrimaryProcess()) continue;

                        if (result.GetValue(listOnlyWindowed) && process.MainWindowHandle == nint.Zero) continue;

                        List<string> columns = [];
                        foreach (var col in columnList)
                        {
                            columns.Add(col switch
                            {
                                ListColumnType.Name => process.ProcessName,
                                ListColumnType.Title => process.MainWindowTitle,
                                ListColumnType.PID => process.Id.ToString(),
                                ListColumnType.StartTime => process.StartTime.ToString(),
                                ListColumnType.Handle => process.Handle.ToString(),
                                ListColumnType.Responding => process.Responding ? "Yes" : "No",
                                ListColumnType.CommandLine => process.GetCommandLine(),
                                _ => string.Empty
                            });
                        }

                        table.AddRow(columns.ToArray());
                    }
                    catch
                    {

                    }
                }

                Console.WriteLine();

                AnsiConsole.Write(table);
            });

            var result = rootCmd.Parse(args);

            result.Invoke();
        }
    }
}