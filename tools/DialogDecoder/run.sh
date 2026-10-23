#!/bin/sh
# Decodes the View properties dialog of a Tekla Structures version, and the pages of the drawing properties dialogs it
# does not have, into page JSON files (see README.md).
#
#   sh tools/DialogDecoder/run.sh [output folder]
#
# The output folder defaults to tools/DialogClassGenerator/Dialogs. Decode into another folder first and compare it with
# compare.py before replacing the files the generator reads.
#
# Environment variables (defaults: Tekla Structures 2025 on the maintainer's machine):
#   TEKLA_ROOT                 installation folder                       D:/Tekla/Structures/2025.0
#   TEKLA_AID                  dialog resources                          $TEKLA_ROOT/bin/TeklaStructures.aid
#   TEKLA_SAMPLE_ATTRIBUTES    folder with VIEW-TEST.vi and TEST.vi saved from the View properties dialog of that version,
#                              DRAWING-TEST.gd from the GA drawing properties and test-ad.ad from the assembly drawing
#                              properties, and OBJECT-TEST.<ext> saved from each property dialog of the drawing editor
#                              (Save as)                                 D:/Tekla/Models/New model/attributes
#   TEKLA_ENVIRONMENTS         .vi, drawing and .dim files for the value statistics, and the sample
#                              common/system/standard.dim                $TEKLA_ROOT/Environments
#   TEKLA_MODELS               more files for the statistics             D:/Tekla/Models
set -e
here=$(cd "$(dirname "$0")" && pwd)
out="${1:-$here/../DialogClassGenerator/Dialogs}"
root="${TEKLA_ROOT:-D:/Tekla/Structures/2025.0}"
aid="${TEKLA_AID:-$root/bin/TeklaStructures.aid}"
attr="${TEKLA_SAMPLE_ATTRIBUTES:-D:/Tekla/Models/New model/attributes}"
environments="${TEKLA_ENVIRONMENTS:-$root/Environments}"
models="${TEKLA_MODELS:-D:/Tekla/Models}"

objects="drtxt sbl lev rev note gln gar gpl gci grt gpg fas fdg idg fhl ldr dprt dsrf po dcha drbr drmsh wls cs detail ga_dim_setup"
for f in "$aid" "$attr/VIEW-TEST.vi" "$attr/TEST.vi" "$attr/DRAWING-TEST.gd" "$attr/test-ad.ad" "$environments/common/system/standard.dim"; do
    if [ ! -f "$f" ]; then
        echo "Missing $f (see tools/DialogDecoder/README.md)" >&2
        exit 1
    fi
done
for e in $objects; do
    if [ ! -f "$attr/OBJECT-TEST.$e" ]; then
        echo "Missing $attr/OBJECT-TEST.$e (see tools/DialogDecoder/README.md)" >&2
        exit 1
    fi
done

mkdir -p "$out"
rm -f "$out"/*.json
# The object samples are positional arguments, so that folder names with spaces stay whole
set -- --file "$environments/common/system/standard.dim"
for e in $objects; do
    set -- "$@" "$attr/OBJECT-TEST.$e"
done
python -I -X utf8 "$here/extract.py" "$aid" "$out" "$attr/VIEW-TEST.vi" "$attr/TEST.vi" --drawing "$attr/DRAWING-TEST.gd" "$attr/test-ad.ad"     "$@" --file-folders "$environments" "$models"
python -I -X utf8 "$here/annotate.py" "$out" "$environments" "$models"
