SHELL := /bin/bash

#todo generate monodeveop makefiles, stick em in here or call them recursively

TEMP_DIR := $(shell mktemp -d)
MOD_NAME := "kesslerchaos"
MOD_DIR := "$(TEMP_DIR)/$(MOD_NAME)/"

release.zip: bin/Release/kesslerchaos.dll toolbaricon.png license readme.md ksppluginframework/license ksp_toolbar/license $(TEMPDIR)
	rm -fv release.zip
	mkdir -v $(MOD_DIR)
	cp -v bin/Release/kesslerchaos.dll $(MOD_DIR)
	cp -v toolbaricon.png $(MOD_DIR)
	cp -v license $(MOD_DIR)
	cp -v readme.md $(MOD_DIR)
	cp -v ksppluginframework/license "$(MOD_DIR)ksp_plugin_framework_license"
	cp -v ksp_toolbar/license "$(MOD_DIR)ksp_toolbar_license"
	pushd $(TEMP_DIR); zip -r release.zip $(MOD_NAME); popd
	mv -v "$(TEMP_DIR)/release.zip" ./release.zip
	rm -rfv $(TEMP_DIR)
clean:
	rm -f release.zip
