LIBRARIES = ["Util", "System.Drawing", "System.Windows.Forms"]
TEST_DLLS = FileList[File.join("lib", "nunit.*.dll")]
SOURCES   = FileList[File.join("src", "**", "*.cs")]
TESTS     = FileList[File.join("test", "**", "*.cs")]
RESOURCES = FileList[File.join("res", "**", "*")]
TARGET    = "BBCiPlayer.plugin"

file TARGET => SOURCES + RESOURCES do |t|
  assembly_info = File.read("src/Properties/AssemblyInfo.cs")
  version = assembly_info[/AssemblyVersion\("([\d\.]+)"\)/, 1]
  new_version = version.split(/\./).map{ |a| a.to_i }
  new_version[-1] += 1
  File.open("src/Properties/AssemblyInfo.cs", "w") do |f|
    f << assembly_info.gsub(version, new_version * ".")
  end
  system(*(
    ["gmcs", "-lib:lib", "-t:library", "-out:#{t.name}"] +
    LIBRARIES.map{ |a| "-r:#{a}.dll" } +
    RESOURCES.map{ |a| "-resource:#{a}" } +
    SOURCES ))
end

file "test.dll" => SOURCES + TESTS + RESOURCES do |t|
  system(*(
    ["gmcs", "-lib:lib", "-t:library", "-out:#{t.name}"] +
    LIBRARIES.map{ |a| "-r:#{a}.dll" } +
    RESOURCES.map{ |a| "-resource:#{a}" } +
    TEST_DLLS.map{ |a| "-r:#{a}" } +
    SOURCES +
    TESTS ))
end

file "plugin.zip" => TARGET do |t|
  rm_f t.name
  system(*(["zip", t.name] + SOURCES + TESTS + RESOURCES + [TARGET]))
end

desc "Package plugin and sources into a zip file"
task :package => "plugin.zip"

desc "Run tests"
task :test => "test.dll" do |t|
  ENV['MONO_PATH'] = "lib"
  system("nunit-console", "test.dll")
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
