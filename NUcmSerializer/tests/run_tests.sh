#!/bin/bash

# Make sure you have reportgenerator tool installed first!
REPORTGEN_CMD="dotnet reportgenerator"
SETTINGS_FILE=".runsettings.xml"
COVERAGE_FILE="coverage.cobertura.xml"

# Retrieve tests output directory
output=$(cat $SETTINGS_FILE)
pattern="<ResultsDirectory>(.*)</ResultsDirectory>"

if [[ ! $output =~ $pattern ]]; then
	echo "ResultsDirectory is missing in settings file: \"$SETTINGS_FILE\"."
	exit 1
fi

output_dir=${BASH_REMATCH[1]}
echo "Value of ResultsDirectory node from \"$SETTINGS_FILE\": \"$output_dir\"."

output=$(dotnet test --collect:"XPlat Code Coverage" \
		     --settings $SETTINGS_FILE \
		     --results-directory=$output_dir)
echo "$output"

# Retrieve parent directory of $COVERAGE_FILE
pattern="($output_dir.*)$COVERAGE_FILE"

if [[ ! $output =~ $pattern ]]; then
	echo "Failed to find \"$COVERAGE_FILE\"'s parent directory."
	exit 1
fi

report_dir=${BASH_REMATCH[1]}

rm -r -f "$output_dir/CoverageReport"
# Invoke indirectly so that $report_dir is removed even if below fails
output=$($REPORTGEN_CMD -reports:"$report_dir/$COVERAGE_FILE" \
			-targetdir:"$output_dir/CoverageReport" \
			-reporttypes:Html)
echo "$output"
echo "Removing: \"$report_dir\"."
rm -r -f $report_dir
echo "Done."
