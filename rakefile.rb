
require 'albacore'

desc "Compiles the solution."
msbuild :compile do |msb|
  
  msb.properties(
    :configuration => 'debug'
    )
  msb.verbosity = "minimal"
  msb.targets :Rebuild
  msb.solution = 'arq.sln'
end
