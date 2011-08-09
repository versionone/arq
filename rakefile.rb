require 'albacore'

@build = {
	:binaries => "arq/bin",
	:config => "debug",
	:core_integration => "../core/lib/arq"
}

task :default => :compile

desc "Compiles the solution."
msbuild :compile do |msb|
  
  msb.properties(
    :configuration => 'debug'
    )
  msb.verbosity = "minimal"
  msb.targets :Rebuild
  msb.solution = 'arq.sln'
end

desc "Launch VisualStudio"
task :sln do
  Thread.new do
    solution = 'arq.sln'
    system %{start "" "#{solution}"}
  end
end

desc "Copies the binaries to Core"
task :publish, :path do |t, args|
  args.with_defaults(:path => @build[:core_integration])
  destination = args.path

  mkdir_p destination
  cp_r File.join(@build[:binaries], @build[:config], '.'), destination
end