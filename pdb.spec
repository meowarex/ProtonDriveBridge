# -*- mode: python ; coding: utf-8 -*-

block_cipher = None

a = Analysis(['build/logic/pdb.py'],
             pathex=['build'],
             binaries=[],
             datas=[
                ('build/ui/pdb.ui', 'ui'),
                ('build/ui/style.css', 'ui'),
                ('build/ui/assets/*.png', 'ui/assets')
             ],
             hiddenimports=[],
             hookspath=[],
             runtime_hooks=[],
             excludes=[],
             win_no_prefer_redirects=False,
             win_private_assemblies=False,
             cipher=block_cipher,
             noarchive=False)

pyz = PYZ(a.pure, a.zipped_data,
          cipher=block_cipher)

exe = EXE(pyz,
          a.scripts,
          a.binaries,
          a.zipfiles,
          a.datas,
          [],
          name='proton-drive-bridge',
          debug=False,
          bootloader_ignore_signals=False,
          strip=False,
          upx=True,
          upx_exclude=[],
          runtime_tmpdir=None,
          console=False)
