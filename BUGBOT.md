# BUGBOT — com.appcharge.paymentlinks

> Watches for licensing correctness and changelog completeness in this UPM package root.

## Documentation
- **Wrong license**: `LICENSE.txt` must be MIT — any other license blocks open distribution.
- **Missing license in package.json**: Add `"license": "MIT"` — consumers depend on this field.
- **Incomplete platform list**: `NOTICE.txt` must name iOS, Android, and WebGL — omissions mislead integrators.
- **Changelog gaps**: Every committed change needs a `CHANGELOG.md` entry — silent changes break semver trust.

## Checklist
- [ ] `LICENSE.txt` contains MIT license text
- [ ] `package.json` has `"license": "MIT"`
- [ ] `NOTICE.txt` lists iOS, Android, and WebGL
- [ ] `CHANGELOG.md` updated for all changes in this PR
