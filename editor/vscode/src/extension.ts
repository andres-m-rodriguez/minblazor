import * as path from 'path';
import { ExtensionContext } from 'vscode';
import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind,
} from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

export function activate(context: ExtensionContext): void {
  const server = context.asAbsolutePath(path.join('server', 'minblazor-lsp.exe'));

  const serverOptions: ServerOptions = {
    run: { command: server, transport: TransportKind.stdio },
    debug: { command: server, transport: TransportKind.stdio },
  };

  const clientOptions: LanguageClientOptions = {
    documentSelector: [{ scheme: 'file', pattern: '**/*.razor' }],
  };

  client = new LanguageClient('minblazor', 'minblazor', serverOptions, clientOptions);
  client.start();
}

export function deactivate(): Thenable<void> | undefined {
  return client?.stop();
}
