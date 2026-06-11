import { mergeConfig } from 'vitest/config';
import { baseVitestConfig } from '@stocker/ts-config';

export default mergeConfig(baseVitestConfig, {
  test: {},
});
