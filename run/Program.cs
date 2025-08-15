using CommandToolsUtilityLibrary;
using FireBlade.WinInteropUtils;
using PeNet;
using PeNet.Header.Pe;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace run
{
    public partial class Program
    {
        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool SetWindowTextW(IntPtr hWnd, string lpString);

        public static void Main(string[] args)
        {
            var rootCmd = new RootCommand("Starts a process from a file path.");

            var filePath = new Argument<string>("PATH")
            {
                Description = "The file path to start.",
                Arity = ArgumentArity.ExactlyOne
            };

            var launchArgs = new Argument<string>("ARGS")
            {
                Arity = ArgumentArity.ZeroOrOne,
                Description = "The arguments to start the process with."
            };

            var shellExecuteEx = new Option<bool>("--noShellExec", "-n")
            {
                Description = "Do not use the Windows shell to start the process. If enabled, you'll have to pass an actual" +
                " executable file, as the Windows shell is required for file associations.",
                Arity = ArgumentArity.ZeroOrOne
            };

            var workingDir = new Option<string>("--workingDir", "-d")
            {
                Description = "The working directory to start the process in.",
                Arity = ArgumentArity.ZeroOrOne
            };

            var user = new Option<string>("--user", "-u")
            {
                Description = "The user to start the process under. Warning! Users like System and TrustedInstaller won't work, because they require a password."
            };

            var passwd = new Option<string>("--password", "-p")
            {
                Description = "The password of the user to start the process under. Requires --user."
            };

            var noMakeWindow = new Option<bool>("--noCreateWindow", "-w")
            {
                Description = "Don't create a window."
            };

            var runAsAdmin = new Option<bool>("--admin", "-a")
            {
                Description = "Start the process as administrator."
            };

            var redirectOutput = new Option<bool>("--redirout", "-o")
            {
                Description = "Redirect the standard output stream (stdout) of the application."
            };

            var redirectInput = new Option<bool>("--redirin", "-i")
            {
                Description = "Redirect the standard input stream (stdin) of the application."
            };

            var redirectError = new Option<bool>("--redirerr", "-e")
            {
                Description = "Redirect the standard error stream of the application."
            };

            var winStyle = new Option<ProcessWindowStyle>("--windowStyle", "-s")
            {
                Description = "Set the window style of the window.",
                DefaultValueFactory = ProcessWindowStyle (ArgumentResult r) =>
                {
                    return ProcessWindowStyle.Normal;
                }
            };

            var envVars = new Option<string>("--environment", "-env")
            {
                // Response files are supported in System.CommandLine and are intended for this exact purpose
                Description = "A JSON string specifying environment variables to start the process with. For longer strings, use a response file (.rsp) with @filename.rsp."
            };

            var overrideEnv = new Option<bool>("--overenv", "-oev")
            {
                Description = "Override the environment variables for the process instead of appending them to the existing ones when combined" +
                " with --environment (-env)."
            };

            var title = new Option<string>("--title", "-t")
            {
                Description = "Override the title of the main window of the application if the application is a GUI app."
            };

            rootCmd.Arguments.Add(filePath);
            rootCmd.Arguments.Add(launchArgs);

            rootCmd.Options.Add(shellExecuteEx);
            rootCmd.Options.Add(workingDir);
            rootCmd.Options.Add(user);
            rootCmd.Options.Add(passwd);
            rootCmd.Options.Add(noMakeWindow);
            rootCmd.Options.Add(runAsAdmin);
            rootCmd.Options.Add(redirectOutput);
            rootCmd.Options.Add(redirectInput);
            rootCmd.Options.Add(redirectError);
            rootCmd.Options.Add(winStyle);
            rootCmd.Options.Add(envVars);
            rootCmd.Options.Add(overrideEnv);
            rootCmd.Options.Add(title);

            rootCmd.Validators.Add(result =>
            {
                if (result.GetValue(passwd) != null && result.GetValue(user) == null)
                {
                    result.AddError("--user or -u must be specified with --password or -p.");
                }
            });

            rootCmd.Validators.Add(result =>
            {
                if (!result.GetValue(shellExecuteEx))
                {
                    if (result.GetValue(redirectInput) || result.GetValue(redirectOutput) || result.GetValue(redirectError))
                    {
                        result.AddError("To redirect the input, output, or error stream(s), --noShellExec or -n must be specified.");
                    }
                }
            });

            rootCmd.Validators.Add(result =>
            {
                if (result.GetValue(overrideEnv) && result.GetValue(envVars) == null)
                {
                    result.AddError("--overenv or -oev must be combined with --environment or -env.");
                }
            });

            rootCmd.SetAction(result =>
            {
                var psi = new ProcessStartInfo
                {
                    ErrorDialog = true,
                    UseShellExecute = !result.GetValue(shellExecuteEx),
                    WorkingDirectory = Environment.CurrentDirectory,
                    ErrorDialogParentHandle = ConsoleTools.GetCurrentConsoleHandle(),
                    CreateNoWindow = result.GetValue(noMakeWindow),
                    RedirectStandardInput = result.GetValue(redirectInput),
                    RedirectStandardOutput = result.GetValue(redirectOutput),
                    RedirectStandardError = result.GetValue(redirectError),
                    WindowStyle = result.GetValue(winStyle)
                };

                foreach (ParseError parseError in result.Errors)
                {
                    Console.Error.WriteLine(parseError.Message);
                }

                if (result.GetValue(filePath) is string fp)
                {
                    psi.FileName = fp;
                }

                if (result.GetValue(launchArgs) is string la)
                {
                    psi.Arguments = la;
                }

                if (result.GetValue(workingDir) is string wd)
                {
                    psi.WorkingDirectory = wd;
                }

                if (result.GetValue(user) is string u)
                {
                    psi.UserName = u;
                    psi.UseShellExecute = false;
                }

                if (result.GetValue(passwd) is string p)
                {
                    psi.PasswordInClearText = p;
                }

                if (result.GetValue(runAsAdmin))
                {
                    psi.Verb = "runas";
                }

                if (result.GetValue(envVars) is string env)
                {
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(env);

                        if (parsed != null)
                        {
                            if (result.GetValue(overrideEnv))
                            {
                                psi.EnvironmentVariables.Clear();
                            }

                            foreach (var kvp in parsed)
                            {
                                psi.EnvironmentVariables[kvp.Key] = kvp.Value;
                            }
                        }
                        else
                        {
                            result.RootCommandResult.AddError("Error while parsing the environment variable JSON data.");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.RootCommandResult.AddError("Error while parsing the environment variable JSON data:\n" +
                            $"({ex.HResult:X}) {ex.Message}");
                    }

                }

                if (psi.FileName != null)
                {
                    try
                    {
                        var proc = Process.Start(psi);

                        if (proc?.HasExited ?? true)
                        {
                            Console.WriteLine("The process was created successfully but exited with code " + (proc?.ExitCode.ToString() ?? "0"));
                        }
                        else if (proc != null)
                        {
                            Console.WriteLine($"The process {proc.Id} was created successfully.");

                            if (result.GetValue(title) is string t)
                            {
                                var file = new PeFile(psi.FileName);

                                if (file.ImageNtHeaders?.OptionalHeader.Subsystem == SubsystemType.WindowsGui)
                                {
                                    if (proc.MainWindowHandle != nint.Zero)
                                    {
                                        SetWindowTextW(proc.MainWindowHandle, t);
                                    }
                                    else
                                    {
                                        // Poll until the window handle is set
                                        while (proc.MainWindowHandle == nint.Zero)
                                        {
                                            if (proc.HasExited) break;

                                            Thread.Sleep(300);
                                            proc.Refresh();
                                        }

                                        SetWindowTextW(proc.MainWindowHandle, t);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Write to stdout
                        Console.WriteLine("An error occured starting the process.\n");
#if DEBUG
                        Console.Error.WriteLine($"({ex.HResult:X}) {ex.Message}");
#endif
                    }
                }
            });

            var result = rootCmd.Parse(args);
            result.Invoke();
        }
    }
}