require "rubygems"
require "bundler"
Bundler.setup
$: << './'

require 'albacore'
require 'rake/clean'
require 'semver'

require 'buildscripts/utils'
require 'buildscripts/paths'
require 'buildscripts/project_details'
require 'buildscripts/environment'

# to get the current version of the project, type 'SemVer.find.to_s' in this rake file.

desc 'generate the shared assembly info'
assemblyinfo :assemblyinfo => ["env:release"] do |asm|
  data = commit_data() #hash + date
  asm.product_name = asm.title = PROJECTS[:nr][:title]
  asm.description = PROJECTS[:nr][:description] + " #{data[0]} - #{data[1]}"
  asm.company_name = PROJECTS[:nr][:company]
  # This is the version number used by framework during build and at runtime to locate, link and load the assemblies. When you add reference to any assembly in your project, it is this version number which gets embedded.
  asm.version = BUILD_VERSION
  # Assembly File Version : This is the version number given to file as in file system. It is displayed by Windows Explorer. Its never used by .NET framework or runtime for referencing.
  asm.file_version = BUILD_VERSION
  asm.custom_attributes :AssemblyInformationalVersion => "#{BUILD_VERSION}", # disposed as product version in explorer
    :CLSCompliantAttribute => false,
    :AssemblyConfiguration => "#{CONFIGURATION}",
    :Guid => PROJECTS[:nr][:guid]
  asm.com_visible = false
  asm.copyright = PROJECTS[:nr][:copyright]
  asm.output_file = File.join(FOLDERS[:src], 'SharedAssemblyInfo.cs')
end


desc "build sln file"
msbuild :msbuild do |msb|
  msb.solution   = FILES[:sln]
  msb.properties :Configuration => CONFIGURATION
  msb.targets    :Clean, :Build
end


task :nr_output => [:msbuild] do
  target = File.join(FOLDERS[:binaries], PROJECTS[:nr][:id])
  copy_files FOLDERS[:nr][:out], "*.{xml,dll,pdb,config}", target
  CLEAN.include(target)
end

task :output => [:nr_output]
task :nuspecs => [:nr_nuspec]

desc "Create a nuspec for 'NLog.Targets.RabbitMQ'"
nuspec :nr_nuspec do |nuspec|
  nuspec.id = "#{PROJECTS[:nr][:nuget_key]}"
  nuspec.version = BUILD_VERSION
  nuspec.authors = "#{PROJECTS[:nr][:authors]}"
  nuspec.description = "#{PROJECTS[:nr][:description]}"
  nuspec.title = "#{PROJECTS[:nr][:title]}"
  # nuspec.projectUrl = 'http://github.com/haf' # TODO: Set this for nuget generation
  nuspec.language = "en-US"
  nuspec.licenseUrl = "http://www.apache.org/licenses/LICENSE-2.0" # TODO: set this for nuget generation
  nuspec.requireLicenseAcceptance = "false"
  
  nuspec.output_file = FILES[:nr][:nuspec]
  nuspec_copy(:nr, "#{PROJECTS[:nr][:id]}.{dll,pdb,xml}")
end

task :nugets => [:"env:release", :nuspecs, :nr_nuget]

desc "nuget pack 'NLog.Targets.RabbitMQ'"
nugetpack :nr_nuget do |nuget|
   nuget.command     = "#{COMMANDS[:nuget]}"
   nuget.nuspec      = "#{FILES[:nr][:nuspec]}"
   # nuget.base_folder = "."
   nuget.output      = "#{FOLDERS[:nuget]}"
end

task :publish => [:"env:release", :nr_nuget_push]

desc "publishes (pushes) the nuget package 'NLog.Targets.RabbitMQ'"
nugetpush :nr_nuget_push do |nuget|
  nuget.command = "#{COMMANDS[:nuget]}"
  nuget.package = "#{File.join(FOLDERS[:nuget], PROJECTS[:nr][:nuget_key] + "." + BUILD_VERSION + '.nupkg')}"
# nuget.apikey = "...."
  nuget.source = URIS[:nuget_offical]
  nuget.create_only = false
end

task :default  => ["env:release", "assemblyinfo", "msbuild", "output", "nugets"]