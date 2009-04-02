LIBRARIES = ["Util", "System.Drawing"]
SOURCES = FileList['src/**/*.cs']

file "IPlayer.plugin" => SOURCES do |t|
  system(
    "gmcs -lib:lib -t:library -out:#{t.name} " +
    LIBRARIES.map{ |a| "-r:#{a}.dll" }.join(" ") + " " +
    SOURCES.join(" ") )
end

task :default => "IPlayer.plugin"
