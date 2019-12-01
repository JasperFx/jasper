require 'json'

APIKEY = ENV['api_key'].nil? ? '' : ENV['api_key']

BUILD_VERSION =  ENV['version'].nil? ? '0.9.15' : ENV['version']
puts "Build version is #{BUILD_VERSION}"


TEMPLATE_VERSION = '0.9.15'
COMPILE_TARGET = ENV['config'].nil? ? "debug" : ENV['config']
RESULTS_DIR = "artifacts"


tc_build_number = ENV["APPVEYOR_BUILD_NUMBER"]
build_revision = tc_build_number || Time.new.strftime('5%H%M')
build_number = "#{BUILD_VERSION}.#{build_revision}"
BUILD_NUMBER = build_number

CI = ENV["CI"].nil? ? false : true

task :ci => [:commands, :pack, :appVeyorPush]
#task :ci => [:default, :commands, :pack, :appVeyorPush]


task :default => [:test, :integrationtests, :commands]
task :full => [:default, :storyteller]




desc "Prepares the working directory for a new build"
task :clean do
  #TODO: do any other tasks required to clean/prepare the working directory
  FileUtils.rm_rf RESULTS_DIR
  FileUtils.mkdir_p RESULTS_DIR
end


desc "Update the version information for the build"
task :version do
  asm_version = build_number

  begin
    commit = `git log -1 --pretty=format:%H`
  rescue
    commit = "git unavailable"
  end
  #puts "##teamcity[buildNumber '#{build_number}']" unless tc_build_number.nil?
  #puts "Version: #{build_number}" if tc_build_number.nil?

  options = {
	:description => '',
	:product_name => 'JasperFx Applications',
	:copyright => 'Copyright 2019 Jeremy D. Miller, et al. All rights reserved.',
	:trademark => commit,
	:version => asm_version,
	:file_version => build_number,
	:informational_version => asm_version

  }

  puts "Writing src/CommonAssemblyInfo.cs..."
	File.open('src/CommonAssemblyInfo.cs', 'w') do |file|
		file.write "using System.Reflection;\n"
		file.write "using System.Runtime.InteropServices;\n"
		file.write "[assembly: AssemblyProduct(\"#{options[:product_name]}\")]\n"
		file.write "[assembly: AssemblyCopyright(\"#{options[:copyright]}\")]\n"
		file.write "[assembly: AssemblyTrademark(\"#{options[:trademark]}\")]\n"
		file.write "[assembly: AssemblyVersion(\"#{options[:version]}\")]\n"
		file.write "[assembly: AssemblyFileVersion(\"#{options[:file_version]}\")]\n"
		file.write "[assembly: AssemblyInformationalVersion(\"#{options[:informational_version]}\")]\n"
	end


end

desc 'Compile the code'
task :compile => [:clean] do
	sh "dotnet restore Jasper.sln"
end

desc 'Run the unit tests'
task :test => [:compile] do
  FileUtils.mkdir_p RESULTS_DIR

	sh "dotnet test src/Jasper.Testing/Jasper.Testing.csproj --no-restore"
	sh "dotnet test src/Jasper.TestSupport.Tests/Jasper.TestSupport.Tests.csproj --no-restore"

end

desc "Integration Tests"
task :integrationtests => [:compile] do
  if OS.windows?
    sh "docker-switch-linux"
  end

  sh "docker-compose up -d"

  #sh "dotnet test src/Jasper.RabbitMQ.Tests/Jasper.RabbitMQ.Tests.csproj --no-restore"
  sh "dotnet test src/Jasper.Persistence.Testing/Jasper.Persistence.Testing.csproj --no-restore"



end

desc 'npm install for Diagnostics'
task :npm_install do
  Dir.chdir("src/Jasper.Diagnostics") do
    sh "yarn"
  end
end

desc 'Try out commands'
task :commands do
#  Dir.chdir("src/Subscriber") do
#    sh "dotnet run -- ?"
#    sh "dotnet run -- export-json-schema obj/schema"
#  end

  Dir.chdir("src/ConsoleApp") do
    sh "dotnet run -- codegen preview"
    sh "dotnet run -- codegen test"
    sh "dotnet run -- codegen write"
    sh "dotnet run -- codegen delete"
  end
end


desc 'Build Nuspec packages'
task :pack do
  Dir.chdir "templates" do
		sh "nuget pack jaspertemplates.nuspec -Version #{TEMPLATE_VERSION}"
	end

  pack_nuget 'Jasper'
  pack_nuget 'Jasper.Persistence.Marten'
  pack_nuget 'Jasper.Persistence.SqlServer'
  pack_nuget 'Jasper.Persistence.Database'
  pack_nuget 'Jasper.Persistence.Postgresql'
  pack_nuget 'Jasper.TestSupport.Storyteller'
  pack_nuget 'Jasper.TestSupport.Alba'
  #pack_nuget 'Jasper.RabbitMQ'
  pack_nuget 'Jasper.JsonCommands'
end

def pack_nuget(project)
  file = "src/#{project}/#{project}.csproj"
  sh "dotnet pack #{file} -o ./artifacts --configuration Release --no-restore"
end

desc "Pushes the Nuget's to AppVeyor"
task :appVeyorPush do
  if !CI
    puts "Not on CI, skipping artifact upload"
    next
  end

  sh "appveyor PushArtifact templates/JasperTemplates.#{TEMPLATE_VERSION}.nupkg"


  Dir.glob('./artifacts/*.*') do |file|
    full_path = File.expand_path file
    full_path = full_path.gsub('/', '\\') if OS.windows?
    cmd = "appveyor PushArtifact #{full_path}"
    puts cmd
    sh cmd
  end
end

desc "Launches VS to the Jasper solution file"
task :sln do
	sh "start Jasper.sln"
end

desc "Run the storyteller specifications"
task :storyteller => [:compile] do


  sh "docker-compose up -d"

  result_output = File.expand_path "#{RESULTS_DIR}/stresults.htm"
  puts "appveyor AddTest Testing -Framework Storyteller -FileName SomeFile -Outcome Skipped"
  Dir.chdir("src/StorytellerSpecs") do
    system "dotnet run -- run -r #{result_output} --tracing verbose"
  end
end

desc "Open the storyteller specification editor"
task :open_st do
  sh "dotnet restore Jasper.sln"

	Dir.chdir("src/StorytellerSpecs") do
	  system "dotnet storyteller"
	end
end

"Gets the documentation assets ready"
task :prepare_docs do
	sh "dotnet restore docs.csproj"
	# this will grow to include the storyteller specs
end

"Launches the documentation project in editable mode"
task :docs => [:prepare_docs] do
	sh "dotnet stdocs run -v #{BUILD_VERSION}"
end

"Exports the documentation to jasperfx.github.io - requires Git access to that repo though!"
task :publish => [:prepare_docs] do
	if Dir.exists? 'doc-target'
		FileUtils.rm_rf 'doc-target'
	end

	Dir.mkdir 'doc-target'
	sh "git clone https://github.com/jasperfx/jasperfx.github.io.git doc-target"


	sh "dotnet stdocs export doc-target Website --version #{BUILD_VERSION}"

	Dir.chdir "doc-target" do
		sh "git add --all"
		sh "git commit -a -m \"Documentation Update for #{BUILD_VERSION}\" --allow-empty"
		sh "git push origin master"
	end




end

desc 'Build and test templates'
task :templates => [:clean] do


	Dir.chdir "templates" do
		sh "nuget pack jaspertemplates.nuspec -Version #{TEMPLATE_VERSION}"
	end

	if Dir.exists? 'template-target'
		FileUtils.rm_rf 'template-target'
	end
	Dir.mkdir 'template-target'

  Dir.chdir "template-target" do
    sh "dotnet new --uninstall JasperTemplates"
    sh "dotnet new --uninstall jasper.service"
    sh "dotnet new --uninstall jasper.http"
    sh "dotnet new --uninstall jasper.rabbitmq"
		sh "dotnet new -i ../templates/JasperTemplates.#{TEMPLATE_VERSION}.nupkg"
    sh "dotnet restore"


    sh "dotnet run"

	end
end


def load_project_file(project)
  File.open(project) do |file|
    file_contents = File.read(file, :encoding => 'bom|utf-8')
    JSON.parse(file_contents)
  end
end

module OS
  def OS.windows?
    (/cygwin|mswin|mingw|bccwin|wince|emx/ =~ RUBY_PLATFORM) != nil
  end

  def OS.mac?
   (/darwin/ =~ RUBY_PLATFORM) != nil
  end

  def OS.unix?
    !OS.windows?
  end

  def OS.linux?
    OS.unix? and not OS.mac?
  end
end
