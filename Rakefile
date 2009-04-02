LIBRARIES = ["Util", "System.Drawing"]
SOURCES   = FileList['src/**/*.cs']
RESOURCES = FileList['res/**/*']

file "IPlayer.plugin" => SOURCES + RESOURCES do |t|
  system([
    "gmcs -lib:lib -t:library -out:#{t.name}",
    LIBRARIES.map{ |a| "-r:#{a}.dll" },
    RESOURCES.map{ |a| "-resource:#{a}" },
    SOURCES ].flatten.join(" "))
end

task :default => "IPlayer.plugin"
