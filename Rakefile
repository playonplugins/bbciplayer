LIBRARIES = ["Util", "System.Drawing", "System.Windows.Forms"]
SOURCES   = FileList[File.join("{src,test}", "**", "*.cs")]
RESOURCES = FileList[File.join("res", "**", "*.dll")]
TARGET    = "BBCiPlayer.plugin"

file TARGET => SOURCES + RESOURCES do |t|
  system(*(
    ["gmcs", "-lib:lib", "-t:library", "-out:#{t.name}"] +
    LIBRARIES.map{ |a| "-r:#{a}.dll" } +
    RESOURCES.map{ |a| "-resource:#{a}" } +
    SOURCES ))
end

desc "Run plugin in debugger"
task :debug => TARGET do |t|
  Dir.chdir("lib") do
    system("mono PluginDebugger.exe " + File.join("..", TARGET))
  end
end

desc "Build the plugin"
task :build => TARGET

task :default => :build
