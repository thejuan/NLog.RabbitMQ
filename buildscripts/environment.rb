require "buildscripts/paths"
require "buildscripts/project_details"
require 'semver'

namespace :env do

  task :common do
    # version management
    fv = version SemVer.find.to_s
    revision = (!fv[3] || fv[3] == 0) ? (ENV['BUILD_NUMBER'] || Time.now.strftime('%j%H')) : fv[3] #  (day of year 0-265)(hour 00-24)

    ENV['BUILD_VERSION'] = BUILD_VERSION = "#{ SemVer.new(fv[0], fv[1], fv[2]).format "%M.%m.%p" }.#{revision.gsub(/^0/,'')}"
    puts "Assembly Version: #{BUILD_VERSION}."
    puts "##teamcity[buildNumber '#{BUILD_VERSION}']" # tell teamcity our decision

    # .net/mono configuration management
    ENV['FRAMEWORK'] = FRAMEWORK = ENV['FRAMEWORK'] || (Rake::Win32::windows? ? "net40" : "mono28")
    puts "Framework: #{FRAMEWORK}"
  end

  # configure the output directories
  task :configure, [:str] do |_, args|
    ENV['CONFIGURATION'] = CONFIGURATION = args[:str]
    FileUtils.rm_rf FOLDERS[:build]
    FOLDERS[:binaries] = File.join(FOLDERS[:build], FRAMEWORK, args[:str].downcase)
    CLEAN.include(File.join(FOLDERS[:binaries], "*"))
  end

  task :set_dirs do


    FOLDERS[:nr][:out] = File.join(FOLDERS[:src], PROJECTS[:nr][:dir], 'bin', CONFIGURATION)
    CLEAN.include(FOLDERS[:nr][:out])

    # for tests
    FOLDERS[:nr][:test_out] = File.join(FOLDERS[:src], PROJECTS[:nr][:test_dir], 'bin', CONFIGURATION)
    FILES[:nr][:test] = File.join(FOLDERS[:nr][:test_out], "#{PROJECTS[:nr][:test_dir]}.dll")
    CLEAN.include(FOLDERS[:test_out])

  end

  task :dir_tasks do
    all_dirs = []

    [:build, :tools, :tests, :nuget, :nuspec].each do |dir|
      directory FOLDERS[dir]
      all_dirs <<  FOLDERS[dir]
    end

    [:out, :nuspec, :test_out].each do |dir|
      [:nr].each{ |k|
        directory FOLDERS[k][dir]
        all_dirs << FOLDERS[k][dir]
      }
    end

    all_dirs.each do |d|
      Rake::Task[d].invoke
    end
  end

  # DEBUG/RELEASE

  desc "set debug environment variables"
  task :debug => [:common] do
    Rake::Task["env:configure"].invoke('Debug')
    Rake::Task["env:set_dirs"].invoke
    Rake::Task["env:dir_tasks"].invoke
  end

  desc "set release environment variables"
  task :release => [:common] do
    Rake::Task["env:configure"].invoke('Release')
    Rake::Task["env:set_dirs"].invoke
    Rake::Task["env:dir_tasks"].invoke
  end

  # FRAMEWORKS

  desc "set net40 framework"
  task :net40 do
    ENV['FRAMEWORK'] = 'net40'
  end
end
